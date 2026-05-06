using Notificacion.Domain.Models;
using Resguardo.Application.Common.Interfaces;
using System.Threading.Channels;

namespace Resguardo.Infrastructure.Background.Email
{
    public class EmailQueue : IEmailQueue
    {
        private readonly Channel<EmailMessage> _queue;

        public EmailQueue()
        {
            _queue = Channel.CreateUnbounded<EmailMessage>();
        }
        public void Enqueue(EmailMessage message)
        {
            _queue.Writer.TryWrite(message);
        }
        public ChannelReader<EmailMessage> Reader => _queue.Reader;
    }
}
