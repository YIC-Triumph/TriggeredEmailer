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
            var incompleteScheduledSessionResult = _dbContext.sessionUserJoins
                .FromSqlRaw("SELECT s.sessID, s.TherapistID, s.Affirmed, s.AbsenceReason, u.UserID, u.UserName, u.EmailAddress FROM vwSessions s INNER JOIN users u ON s.TherapistID = u.UserID")
                .Where(s => s.Affirmed == 0)
                .Where(s => s.AbsenceReason == null || s.AbsenceReason.Length < 1)
                .ToList();

            foreach (var session in incompleteScheduledSessionResult)
            {
                var message = $"Dear {session.UserName},\n\nYou have an incomplete session scheduled for this session id: {session.sessID}.\n\nKindly edit the incomplete scheduled session through this link: {_appSettings.Domain}SessionEdit.aspx?SS_ID={session.sessID}&ShowAll=False&QuickAdd=False&returnTo=caseload-clientlist&Staff_ID={session.TherapistID}&AllAssmts= \n\nThank you,\n\nTriumph";

                await _emailService.SendEmail(session.EmailAddress, "Incomplete Session", message);
            }
        }
    }
}