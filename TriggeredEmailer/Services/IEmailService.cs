namespace TriggeredEmailer.Services
{
    public interface IEmailService
    {
        Task SendEmail(string toEmail, string subject, string html);
    }
}
