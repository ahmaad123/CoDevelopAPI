using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Options;
using CoDevelopAPI.Models;
using CoDevelopAPI.Services.Interfaces;

namespace CoDevelopAPI.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                _logger.LogInformation($"Attempting to send email to {toEmail} via Gmail");

                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
                email.To.Add(MailboxAddress.Parse(toEmail));
                email.Subject = subject;

                var builder = new BodyBuilder
                {
                    HtmlBody = body
                };
                email.Body = builder.ToMessageBody();

                using var smtp = new SmtpClient();

                // Accept all SSL certificates
                smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;
                //smtp.Timeout = 30000; 

                try
                {
                    // Try STARTTLS on port 587
                    await smtp.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to connect on port 587, trying SSL on port 465");
                    // Fallback to SSL on port 465
                    await smtp.ConnectAsync("smtp.gmail.com", 465, SecureSocketOptions.SslOnConnect);
                }

                // Authenticate with Gmail
                await smtp.AuthenticateAsync(
                    _emailSettings.SenderEmail,
                    _emailSettings.SenderPassword);

                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

                _logger.LogInformation($"Email sent successfully to {toEmail}");
                return true;
            }
            catch (AuthenticationException ex)
            {
                _logger.LogError(ex, "Gmail authentication failed. Ensure you're using an App Password.");
                throw new Exception("Gmail authentication failed. Use an App Password from Google Account settings.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {toEmail}: {ex.Message}");
                throw;
            }
        }
        public async Task<bool> SendPasswordEmailAsync(string toEmail, string userName, string password)
        {
            var subject = "Your CoDevelop Account Password";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #3b82f6;'>Welcome to CoDevelop System</h2>
                        <p>Hello {userName},</p>
                        <p>Your account has been created successfully. Below are your login credentials:</p>
                        
                        <div style='background-color: #f3f4f6; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                            <p><strong>Email:</strong> {toEmail}</p>
                            <p><strong>Password:</strong> <code style='background-color: #e5e7eb; padding: 4px 8px; border-radius: 4px;'>{password}</code></p>
                        </div>
                        
                        <p style='color: #ef4444;'><strong>Important:</strong> Please change your password after your first login.</p>
                        <p>Click the link below to access the system:</p>
                        <a href='http://localhost:3000/login' 
                           style='display: inline-block; padding: 12px 24px; background-color: #3b82f6; color: white; text-decoration: none; border-radius: 6px;'>
                            Login to CoDevelop
                        </a>
                        
                        <hr style='margin: 30px 0; border: 1px solid #e5e7eb;'>
                        <p style='color: #6b7280; font-size: 12px;'>
                            This is an automated message from CoDevelop System. Please do not reply to this email.
                        </p>
                    </div>
                </body>
                </html>";

            return await SendEmailAsync(toEmail, subject, body);
        }

        public async Task<bool> SendWelcomeEmailAsync(string toEmail, string userName)
        {
            var subject = "Welcome to CoDevelop System";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #3b82f6;'>Welcome to CoDevelop System</h2>
                        <p>Hello {userName},</p>
                        <p>Your account has been created successfully. You can now access the CoDevelop system.</p>
                        <p>Click the link below to get started:</p>
                        <a href='http://localhost:3000/login' 
                           style='display: inline-block; padding: 12px 24px; background-color: #3b82f6; color: white; text-decoration: none; border-radius: 6px;'>
                            Access CoDevelop
                        </a>
                    </div>
                </body>
                </html>";

            return await SendEmailAsync(toEmail, subject, body);
        }
    }
}
