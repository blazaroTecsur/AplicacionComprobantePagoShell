using Microsoft.Extensions.Options;
using Notificacion.Application;
using Notificacion.Domain.Models;
using System.Net;
using System.Net.Mail;

namespace Notificacion.Infrastructure.Email
{
    public class SmtpEmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        public SmtpEmailService(IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
        }
        public async Task SendAsync(EmailMessage message, CancellationToken ct = default)
        {
            using var mail = new MailMessage
            {
                From = new MailAddress(_settings.From, _settings.FromName),
                Subject = message.Subject,
                Body = message.Body,
                IsBodyHtml = message.IsHtml
            };

            foreach (var to in message.To) mail.To.Add(to);
            if (message.Cc != null) foreach (var cc in message.Cc) mail.CC.Add(cc);
            if (message.Bcc != null) foreach (var bcc in message.Bcc) mail.Bcc.Add(bcc);
            if (message.Attachments != null)
            {
                foreach (var att in message.Attachments)
                {
                    var stream = new MemoryStream(att.Value);
                    mail.Attachments.Add(new Attachment(stream, att.Key));
                }
            }

            using var smtp = new SmtpClient(_settings.Host, _settings.Port)
            {
                Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                EnableSsl = _settings.EnableSsl
            };

            await smtp.SendMailAsync(mail, ct);
        }
    }
}