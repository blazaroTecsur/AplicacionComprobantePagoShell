using Maestros.Abstractions.Interfaces;
using Maestros.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Maestros.Infrastructure.DependencyInjection
{
    public static class MaestrosServiceCollectionExtensions
    {
        public static IServiceCollection AddMaestros(
            this IServiceCollection services, IConfiguration config)
        {
            services.Configure<MaestrosSettings>(config.GetSection("ApiMaestros"));

            services.AddHttpClient<IMaestrosProveedorService, MaestrosProveedorService>(
                (sp, client) =>
                {
                    var settings = sp.GetRequiredService<IOptions<MaestrosSettings>>().Value;
                    client.BaseAddress = new Uri(settings.BaseUrl);
                });

            services.AddHttpClient<IMaestrosEmpleadoService, MaestrosEmpleadoService>(
                (sp, client) =>
                {
                    var settings = sp.GetRequiredService<IOptions<MaestrosSettings>>().Value;
                    client.BaseAddress = new Uri(settings.BaseUrl);
                });

            services.AddHttpClient<IMaestrosCuentaContableService, MaestrosCuentaContableService>(
                (sp, client) =>
                {
                    var settings = sp.GetRequiredService<IOptions<MaestrosSettings>>().Value;
                    client.BaseAddress = new Uri(settings.BaseUrl);
                });

            services.AddHttpClient<IMaestrosCatalogoUnidadService, MaestrosCatalogoUnidadService>(
                (sp, client) =>
                {
                    var settings = sp.GetRequiredService<IOptions<MaestrosSettings>>().Value;
                    client.BaseAddress = new Uri(settings.BaseUrl);
                });

            return services;
        }
    }
}
