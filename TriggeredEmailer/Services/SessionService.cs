using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Data;
using System.Text;
using TriggeredEmailer.Constants;
using TriggeredEmailer.Data;
using TriggeredEmailer.Helpers;
using TriggeredEmailer.Interfaces;
using TriggeredEmailer.Models;

namespace TriggeredEmailer.Services
{
    internal class SessionService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IEmailService _emailService;
        private readonly AppSettings _appSettings;

        public SessionService(
            IOptions<AppSettings> appSettings, 
            ApplicationDbContext dbContext, 
            IEmailService emailService)
        {
            _appSettings = appSettings.Value ?? throw new ArgumentNullException(nameof(appSettings));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _emailService = emailService;
        }

        public async Task SendEmailToProviderForIncompleteSessionAsync()
        {
            string technicalSupportEmail = "Technicalsupport@triumphaba.com";
            var incompleteScheduledSessionResult = _dbContext.sessionUserJoins
                .FromSqlRaw("SELECT s.sessID, s.TherapistID, s.Affirmed, s.notes, s.AbsenceReason, s.starttime, s.endtime, s.SessHrs, s.sessdate, s.FirstName As StudentFirstName, s.LastName As StudentLastName, u.UserID, u.UserName, u.EmailAddress, u.FirstName As ProviderFirstName, u.LastName As ProviderLastName FROM vwSessions s INNER JOIN vwStaff u ON s.TherapistID = u.LoginID WHERE s.SessionStatus NOT IN (3, 4)")
                .Where(s => s.notes == "")
                .Where(s => s.notes == null)
                .Where(s => s.sessdate < DateTime.Now)
                .ToList();
            
            foreach (var session in incompleteScheduledSessionResult)
            {
                string editSessionLink = $"{_appSettings.Domain}SessionEdit.aspx?SS_ID={session.sessID}&ShowAll=False&QuickAdd=False&returnTo=caseload-clientlist&Staff_ID={session.TherapistID}&AllAssmts=";

                var message = $@"
                    Dear {session.ProviderFirstName} {session.ProviderLastName},

                    Your session with {session.StudentFirstName} {session.StudentLastName} from {session.starttime} to {session.endtime} has ended.

                    <a href=""{editSessionLink}"">Click on this link to enter in and complete your session</a>.
                    
                    If you have any questions, feel free to reach out to {technicalSupportEmail} x613 for assistance.
                    
                    - Triumph Behavior Support.";

                await _emailService.SendEmail(session.EmailAddress, "Incomplete Session", message);
            }
        }
    }
}