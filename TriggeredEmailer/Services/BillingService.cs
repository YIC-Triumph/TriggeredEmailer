using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;
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
        private readonly IFileLogger<BillingService> _logger;
        public BillingService(
            ApplicationDbContext dbContext,
            IVWSessionsService vWSessionsService,
            IVWStaffService vwStaffService,
            IFileLogger<BillingService> logger
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
        private DateTime _sunday = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek);

        /// <summary>
        /// invoke auto billing
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public async Task ConfigureSendBilling(Roles role)
        {
            var sb = new StringBuilder();

            //1 get starting date of billing last Week
            var lastSunday = _sunday.AddDays(-7);
            var saturdayDate = lastSunday.AddMinutes(_daysInterval);

            //2 get all sessions from vwsession
            var vwSessions = await _vwSessionsService.GetAll(lastSunday, saturdayDate, role);

            //3 list of valid session per therapist
            var validSessions = new Dictionary<int, List<vwSession>>();

            var gsessionPerProviderGrp = vwSessions.GroupBy(t => t.TherapistID);

            foreach (var sessions in gsessionPerProviderGrp)
            {
                var hasIncompleteSession = sessions.Any(session =>
                {
                    SessionStatus sessionStatus;
                    if (Enum.TryParse(session.SessionStatus.ToString(), out sessionStatus))
                        if (sessionStatus < SessionStatus.Absent)
                        {
                            sb.AppendLine($"Session {session.SessID} is {sessionStatus.ToString()}");
                            sb.AppendLine($"\tTherapist ID: {session.TherapistID}");
                            sb.AppendLine($"\tStudent ID: {session.StudentID}");
                            return true;
                        }

                    return false;
                });

                if (!hasIncompleteSession)
                    validSessions.Add((int)sessions.Key, sessions.ToList());
            }

            if (!validSessions.Any())
            {
                await _logger.WriteLog(sb.ToString(), LogType.File, LogLevel.Information);
                return;
            }

            await ExecuteBilling(validSessions);
        }

        /// <summary>
        /// This method executes billing
        /// </summary>
        /// <param name="validSessions"></param>
        /// <returns></returns>
        private async Task ExecuteBilling(Dictionary<int, List<vwSession>> validSessions)
        {
            var logLevel = LogLevel.Information;
            var args = new Exception();
            StringBuilder sb = new StringBuilder();

            var providers = await _vwStaffService.GetAll();

            try
            {
                foreach (var sess in validSessions)
                {
                    var role = providers.Where(p => p.LoginID == sess.Key).FirstOrDefault();
                    var pIds = string.Join(',', sess.Value.Select(t => t.SessID));

                    //if ROLE is BCBA, validate if any of the sessions are supervision sessions that theres a bt session being supervised
                    if (role?.R_ID == (int)Roles.BCBA)
                    {
                        var status = await _dbContext.Database.SqlQuery<string>($"exec sp_CheckIfSupervisedSessionEntered @sessIDs={pIds}").ToListAsync();

                        var sessionStat = status.FirstOrDefault();
                        if (status.Count > 0)
                        {
                            switch (sessionStat)
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
                        await _dbContext.Database.ExecuteSqlAsync($"exec sp_CheckIfSessSupervisedOnHold @sessIDs={pIds}");

                    //need to split by student
                    //calculate total hours and pay and then insert into the table
                    var dsSesStu = _dbContext.SessionStudents.FromSql($"exec sp_SubmitToBilling_GroupsessByStudent @strIDs={pIds}");

                    //store billing to invoice
                    foreach (var item in dsSesStu)
                        _dbContext.BillingAmounts.FromSql($"exec sp_SubmitToBilling @SessIds={item.strIDs}, @ProviderID={sess.Key}, @StudentID={item.studentID}, @PayPeriodID={sess.Value.Select(p => p.PP_ID)}");


                    sb.AppendLine($"Successfull billing for provider {sess.Key}, with sessions {pIds}");
                    await _logger.WriteLog(sb.ToString(), LogType.File, logLevel);
                }
            }
            catch (Exception ex)
            {
                logLevel = LogLevel.Error;
                sb.AppendLine(ex.Message);
                args = ex;
            }

            //log to file
            await _logger.WriteLog(sb.ToString(), LogType.File, logLevel, args);
        }
    }
}
