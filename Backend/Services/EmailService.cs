using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using PruebaTecnica_MonoLegal.Configuration;
using PruebaTecnica_MonoLegal.Services.Interfaces;

namespace PruebaTecnica_MonoLegal.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> options)
        {
            _settings = options.Value;
        }

        public async Task SendEmailAsync(
            string recipient,
            string subject,
            string body,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_settings.SenderEmail))
            {
                throw new InvalidOperationException(
                    "Debes configurar SenderEmail.");
            }

            if (string.IsNullOrWhiteSpace(_settings.Username))
            {
                throw new InvalidOperationException(
                    "Debes configurar Username.");
            }

            if (string.IsNullOrWhiteSpace(_settings.Password))
            {
                throw new InvalidOperationException(
                    "Debes configurar la contraseña de aplicación de Gmail.");
            }

            var message = new MimeMessage();

            message.From.Add(
                new MailboxAddress(
                    _settings.SenderName,
                    _settings.SenderEmail));

            message.To.Add(
                MailboxAddress.Parse(recipient));

            message.Subject = subject;

            message.Body = new BodyBuilder
            {
                TextBody = body
            }.ToMessageBody();

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(
                _settings.SmtpServer,
                _settings.Port,
                SecureSocketOptions.StartTls,
                cancellationToken);

            await smtp.AuthenticateAsync(
                _settings.Username,
                _settings.Password,
                cancellationToken);

            await smtp.SendAsync(
                message,
                cancellationToken);

            await smtp.DisconnectAsync(
                true,
                cancellationToken);
        }
    }
}