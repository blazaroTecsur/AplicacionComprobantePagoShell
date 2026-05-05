using Notificacion.Domain.Models;

namespace Notificacion.Application
{
    public interface IEmailService
    {
        Task SendAsync(EmailMessage message, CancellationToken ct = default);
    }
}
