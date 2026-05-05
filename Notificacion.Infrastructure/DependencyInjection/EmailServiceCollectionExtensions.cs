using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notificacion.Application;
using Notificacion.Infrastructure.Email;

namespace Notificacion.Infrastructure.DependencyInjection
{
    public static class EmailServiceCollectionExtensions
    {
        public static IServiceCollection AddEmail(this IServiceCollection services, IConfiguration config)
        {
            var provider = config["Email:Provider"] ?? "SMTP";

            if (provider == "SMTP")
            {
                services.Configure<EmailSettings>(config.GetSection("Email:Smtp"));
                services.AddScoped<IEmailService, SmtpEmailService>();
            }
            return services;
        }
    }
}