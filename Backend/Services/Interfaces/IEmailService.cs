namespace PruebaTecnica_MonoLegal.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(
            string recipient,
            string subject,
            string body,
            CancellationToken cancellationToken = default);
    }
}