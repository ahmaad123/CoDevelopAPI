namespace CoDevelopAPI.Models
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; } = "smtp.gmail.com";
        public int SmtpPort { get; set; } = 587;
        public string SenderEmail { get; set; }
        public string SenderPassword { get; set; }
        public string SenderName { get; set; } = "CoDevelop System";
        public bool UseSSL { get; set; } = true;
    }
}