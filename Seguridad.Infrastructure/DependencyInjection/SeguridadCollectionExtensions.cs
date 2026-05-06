using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using Seguridad.Abstractions.Interfaces;
using Seguridad.Infrastructure.Handler.Authentication;
using Seguridad.Infrastructure.Handler.Authorization;
using Seguridad.Infrastructure.Middlewares;
using Seguridad.Infrastructure.Services;

namespace Seguridad.Infrastructure.DependencyInjection
{
    public static class SeguridadCollectionExtensions
    {
        public static IServiceCollection AddSeguridad(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<SeguridadSetting>(config.GetSection("ApiSettings:Seguridad"));            
            services.AddSingleton<IMsalHttpClientFactory, NoProxyMsalHttpClientFactory>();
            services.AddSingleton<SeguridadTokenService>();
            services.AddHttpClient<ISeguridadService, SeguridadService>();

            services.AddAuthentication("Internal").AddScheme<AuthenticationSchemeOptions, 
                InternalAuthHandler>("Internal", null);
            services.AddScoped<IAuthorizationHandler, PermissionHandler>();
            services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
            services.AddScoped<IClaimsTransformation, PermisosClaimsTransformation>();
            services.AddScoped<IUsuarioContexto, UsuarioContexto>();

            return services;
        }
    }
}