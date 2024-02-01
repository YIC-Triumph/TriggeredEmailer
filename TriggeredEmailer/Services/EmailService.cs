using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using TriggeredEmailer.Helpers;

namespace TriggeredEmailer.Services
{
    public class EmailService : IEmailService
    {
        private readonly AppSettings _appSettings;

        public EmailService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value ?? throw new ArgumentNullException(nameof(appSettings));
        }

        public async Task SendEmail(string toEmail, string subject, string message)
        {
            var client = new SendGridClient(_appSettings.SendGridApiKey);
            var from = new EmailAddress(_appSettings.EmailFrom, "Incomplete Session Alert");
            var to = new EmailAddress(toEmail);
            var plainTextContent = message;
            var htmlContent = message;
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

            try
            {
                var response = await client.SendEmailAsync(msg);
                Console.WriteLine($"Email sent successfully. Status code: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
            }
        }
    }
}
