﻿using Microsoft.EntityFrameworkCore;
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

        //start of billing
        private DateTime _lastSunday => _sunday.AddDays(-7);
        //end of billing
        private DateTime _lastSaturday => _lastSunday.AddMinutes(_daysInterval);

        /// <summary>
        /// invoke auto billing
        /// <param name="role"></param>
        /// </summary>
        public async Task ConfigureSendBilling(Roles role)
        {
            var sb = new StringBuilder();

            //2 get all sessions from vwsession
            var vwSessions = await _vwSessionsService.GetAll(_lastSunday, _lastSaturday, role);

            //3 list of valid session per therapist
            var validSessions = new Dictionary<int, List<vwSession>>();

            var gsessionPerProviderGrp = vwSessions.GroupBy(t => t.TherapistID);

            sb.AppendLine("Billing was not performed due to the following sessions: ");
            sb.AppendLine($"Billing date: {_lastSunday.ToLongDateString()} - {_lastSaturday.ToLongDateString()}");
            foreach (var sessions in gsessionPerProviderGrp)
            {
                var hasIncompleteSession = sessions.Any(session =>
                {
                    SessionStatus sessionStatus;
                    if (Enum.TryParse(session.SessionStatus.ToString(), out sessionStatus))
                        if (sessionStatus < SessionStatus.Absent)
                        {
                            sb.AppendLine($"Session {session.SessID} is {sessionStatus.ToString()}");
                            sb.AppendLine($"\tSession date: {session.Sessdate}");
                            sb.AppendLine($"\tProvider name: {session.ProviderFName}({session.TherapistID})");
                            sb.AppendLine($"\tStudent name: {session.StudentFName}({session.StudentID})");
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
                    
            sb.AppendLine("Perform Billing:");
            try
            {
                foreach (var sess in validSessions)
                {
                    sb = new StringBuilder();
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
                    var studentSessions = _dbContext.SessionStudents.FromSql($"exec sp_SubmitToBilling_GroupsessByStudent @strIDs={pIds}").ToList();

                    //store billing to invoice
                    foreach (var item in studentSessions)
                    {
                        int pp_id = sess.Value.Where(s => s.StudentID == item.studentID).Select(p => p.PP_ID).FirstOrDefault();
                        await _dbContext.Database.ExecuteSqlAsync($"exec sp_SubmitToBilling @SessIds={item.strIDs}, @ProviderID={sess.Key}, @StudentID={item.studentID}, @PayPeriodID={pp_id}");
                    }

                    var session = sess.Value.FirstOrDefault();
                    sb.AppendLine($"Successfull billing for {role?.LastName}, {role?.FirstName}({role?.LoginID}) with sessions {pIds}");
                    sb.AppendLine($"Billing date: {_lastSunday.ToLongDateString()} - {_lastSaturday.ToLongDateString()}");
                    sb.AppendLine($"\tStudent name: {session?.StudentFName}({session?.StudentID})");
                    await _logger.WriteLog(sb.ToString(), LogType.File, logLevel);
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine(ex.Message);
                args = ex;

                //log to file
                await _logger.WriteLog(sb.ToString(), LogType.File, LogLevel.Error, args);
            }

        }
    }
}
