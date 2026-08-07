namespace CoDevelopAPI.Services.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string toEmail, string subject, string body);
        Task<bool> SendPasswordEmailAsync(string toEmail, string userName, string password);
        Task<bool> SendWelcomeEmailAsync(string toEmail, string userName);
    }
}