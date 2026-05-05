using Notificacion.Domain.Models;

namespace Resguardo.Application.Interfaces.Background
{
    public interface IEmailQueue
    {
        void Enqueue(EmailMessage message);
    }
}