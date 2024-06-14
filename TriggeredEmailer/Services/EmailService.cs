using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using TriggeredEmailer.Constants;
using TriggeredEmailer.Helpers;
using TriggeredEmailer.Interfaces;

namespace TriggeredEmailer.Services
{
    public class EmailService : IEmailService
    {
        private readonly AppSettings _appSettings;

        private readonly IFileLogger<EmailService> _logger;

        public EmailService(IOptions<AppSettings> appSettings, IFileLogger<EmailService> logger)
        {
            _appSettings = appSettings.Value ?? throw new ArgumentNullException(nameof(appSettings));
            _logger = logger?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task SendEmail(string toEmail, string subject, string message)
        {
            var client = new SendGridClient(_appSettings.SendGridApiKey);
            var from = new EmailAddress(_appSettings.EmailFrom, "Incomplete Session Alert");
            var to = new EmailAddress(toEmail);
            var plainTextContent = message;
            var htmlContent = message;
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var args = new Exception();

            try
            {
                var response = await client.SendEmailAsync(msg);

                await _logger.WriteLog($"Email sent successfully to {toEmail}. Status code: {response.StatusCode}", LogType.File, LogLevel.Information, args);
            }
            catch (Exception ex)
            {
                await _logger.WriteLog($"Error sending email to {toEmail}. Error: {ex.Message}", LogType.File, LogLevel.Error, args);
            }
        }
    }
}
