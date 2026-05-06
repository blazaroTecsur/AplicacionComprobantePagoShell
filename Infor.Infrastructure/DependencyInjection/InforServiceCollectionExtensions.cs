using Infor.Abstractions.Interfaces;
using Infor.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Notificacion.Infrastructure.DependencyInjection
{
    public static class InforServiceCollectionExtensions
    {
        public static IServiceCollection AddInfor(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<InforSettings>(config.GetSection("ApiSettings:Infor"));
            services.AddScoped<IInforTokenService, InforTokenService>();
            services.AddScoped<IInforIdoService, InforIdoService>();
            return services;
        }
    }
}