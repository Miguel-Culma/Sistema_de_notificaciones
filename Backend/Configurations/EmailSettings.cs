namespace PruebaTecnica_MonoLegal.Configuration
{
    public class EmailSettings
    {
        public string SenderName { get; set; } = "MonoLegal";

        public string SenderEmail { get; set; } = string.Empty;

        public string SmtpServer { get; set; } = "smtp.gmail.com";

        public int Port { get; set; } = 587;

        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}