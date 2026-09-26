using System.Net;
using System.Net.Mail;

namespace CarPoolingApp.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var host = _config["EmailSettings:SmtpServer"] ?? "";
            var port = int.Parse(_config["EmailSettings:Port"] ?? "587");
            var username = _config["EmailSettings:Username"] ?? "";
            var password = _config["EmailSettings:Password"] ?? "";
            var sender = _config["EmailSettings:SenderEmail"] ?? "";

            var smtp = new SmtpClient
            {
                Host = host,
                Port = port,
                EnableSsl = true,
                Credentials = new NetworkCredential(username, password)
            };

            var message = new MailMessage(
                sender,
                toEmail,
                subject,
                body
            );

            await smtp.SendMailAsync(message);
        }
    }
}