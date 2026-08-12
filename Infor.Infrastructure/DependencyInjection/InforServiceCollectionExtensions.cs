using Infor.Abstractions.Interfaces;
using Infor.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infor.Infrastructure.DependencyInjection
{
    public static class InforServiceCollectionExtensions
    {
        public static IServiceCollection AddInfor(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<InforSettings>(config.GetSection("ApiSettings:Infor"));
            services.AddHttpClient<IInforTokenService, InforTokenService>()
            .ConfigurePrimaryHttpMessageHandler(() =>
                new HttpClientHandler
                {
                    UseProxy = false,
                    Proxy = null
                });
            services.AddHttpClient<IInforIdoService, InforIdoService>()
            .ConfigurePrimaryHttpMessageHandler(() =>
                new HttpClientHandler
                {
                    UseProxy = false,
                    Proxy = null
                });

            return services;
        }
    }
}