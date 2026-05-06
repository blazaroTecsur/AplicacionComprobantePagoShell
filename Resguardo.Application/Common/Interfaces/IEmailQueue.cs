using Notificacion.Domain.Models;

namespace Resguardo.Application.Common.Interfaces
{
    public interface IEmailQueue
    {
        void Enqueue(EmailMessage message);
    }
}