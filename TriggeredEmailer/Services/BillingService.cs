using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TriggeredEmailer.Constants;
using TriggeredEmailer.Data;
using TriggeredEmailer.Interfaces;
using TriggeredEmailer.Models;

namespace TriggeredEmailer.Services
{
    public class BillingService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IVWSessionsService _vwSessionsService;
        private readonly IVWStaffService _vwStaffService;
        private readonly ILogger<BillingService> _logger;
        public BillingService(
            ApplicationDbContext dbContext,
            IVWSessionsService vWSessionsService,
            IVWStaffService vwStaffService,
            ILogger<BillingService> logger
            )
        {
            _dbContext = dbContext;
            _vwSessionsService = vWSessionsService;
            _vwStaffService = vwStaffService;
            _logger = logger;
        }

        //get end date of billing, days interval, 6 days, time not relevant
        private const uint _daysInterval = (60 * 24 * 6);

        //reset day on Sunday
        private DateTime _sunday = DateTime.UtcNow.AddDays(-(int)DateTime.UtcNow.DayOfWeek);

        /// <summary>
        /// invoke auto billing
        /// </summary>
        /// <returns></returns>
        public async Task ConfigureSendBilling()
        {
            //1 get starting date of billing last Week
            var lastSunday = _sunday.AddDays(-7);
            var saturdayDate = lastSunday.AddMinutes(_daysInterval);

            //2 get all sessions from vwsession
            var vwSessions = await _vwSessionsService.GetAll(lastSunday, saturdayDate);

            //3 list of valid session per therapist
            var validSessions = new List<KeyValuePair<int, List<vwSession>>>();

            var gsessionPerProviderGrp = vwSessions.GroupBy(t => t.TherapistID);

            foreach (var sessions in gsessionPerProviderGrp)
            {
                var hasIncompleteSession = sessions.Any(session =>
                {
                    SessionStatus sessionStatus;
                    if (Enum.TryParse<SessionStatus>(session.SessionStatus.ToString(), out sessionStatus))
                        if (sessionStatus != SessionStatus.Absent && sessionStatus != SessionStatus.Completed) return true;

                    return false;
                });

                if (!hasIncompleteSession)
                    validSessions.Add(new KeyValuePair<int, List<vwSession>>((int)sessions.Key, sessions.ToList()));
            }

            if (!validSessions.Any()) return;

            await ExecuteParallelBilling(validSessions);
        }

        /// <summary>
        /// This method executes parallel tasks for billing to improve performance
        /// </summary>
        /// <param name="validSessions"></param>
        /// <returns></returns>
        private async Task ExecuteParallelBilling(List<KeyValuePair<int, List<vwSession>>> validSessions)
        {
            StringBuilder sb = new StringBuilder();

            CancellationTokenSource cts = new CancellationTokenSource();
            var token = cts.Token;

            var providers = await _vwStaffService.GetAll();

            try
            {
                await Task.Run(async () =>
                {
                    await Parallel.ForEachAsync(validSessions, async (sess, token) =>
                    {
                        token.ThrowIfCancellationRequested();

                        Console.WriteLine("start :" + sess.Key + ": " + sess.Value);
                        var role = providers.Where(p => p.LoginID == sess.Key).FirstOrDefault();
                        var pIds = string.Join(',', sess.Value.Select(t => t.sessID));

                        //if ROLE is BCBA, validate if any of the sessions are supervision sessions that theres a bt session being supervised
                        if (role?.R_ID == (int)Roles.BCBA)
                        {
                            var status = _dbContext.Database.SqlQuery<string>($"exec sp_CheckIfSupervisedSessionEntered @sessIDs={pIds}");

                            var sessionStat = await status.ToListAsync();
                            if (sessionStat.Count > 0)
                            {
                                switch (sessionStat.FirstOrDefault())
                                {
                                    case "no session":
                                        //Missing-Supervised
                                        sb.AppendLine($"{role.LastName} {role.FirstName} cannot submit to billing because BT did not sign his/her session({pIds}) that he/she supervised.");
                                        break;
                                    case "not submitted":
                                        //HOLD
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                        //if ROLE is BT, if any of these sessions were supervised and put on hold we need to update then now that we are billing
                        else if (role?.R_ID == (int)Roles.BT)
                        {
                            _dbContext.Database.ExecuteSql($"exec sp_CheckIfSessSupervisedOnHold @sessIDs={pIds}");
                        }

                        //need to split by student
                        //calculate total hours and pay and then insert into the table  
                        var dsSesStu = _dbContext.SessionStudents.FromSql($"exec sp_SubmitToBilling_GroupsessByStudent @strIDs={pIds}");

                        //store billing to invoice
                        foreach (var item in dsSesStu)
                        {
                            _dbContext.BillingAmounts.FromSql($"exec sp_SubmitToBilling @SessIds={item.strIDs}, @ProviderID={sess.Key}, @StudentID={item.studentID}, @PayPeriodID={30}");
                        }
                    });

                });
            }
            catch (Exception ex)
            {
                //log in console
                _logger.LogError(ex.Message, ex);
                //TODO: Log to database;
            }
            if (sb.Length > 0)
            {
                //TODO: Log to database;
                _logger.LogInformation(sb.ToString());
            }
        }
    }
}
