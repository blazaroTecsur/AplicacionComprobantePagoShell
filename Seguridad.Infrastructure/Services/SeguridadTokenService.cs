using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using System.Collections.Concurrent;

namespace Seguridad.Infrastructure.Services
{
    public class SeguridadTokenService
    {
        private readonly SecuritySetting _security;
        private readonly ApplicationSetting _application;
        private readonly IMsalHttpClientFactory _httpFactory;
        private readonly ConcurrentDictionary<string, IConfidentialClientApplication> _clients = new();
        public SeguridadTokenService(
            IMsalHttpClientFactory httpFactory,
            IOptions<SecuritySetting> security,
            IOptions<ApplicationSetting> application)
        {
            _security = security.Value;
            _application = application.Value;
            _httpFactory = httpFactory;
        }
        public async Task<string> GetTokenAsync(string type)
        {
            var scope = GetScope(type);
            var config = GetClient(type);
            var app =
                _clients.GetOrAdd(type.ToString(),
                    _ =>
                        ConfidentialClientApplicationBuilder
                            .Create(config.ClientId)
                            .WithClientSecret(config.ClientSecret)
                            .WithAuthority(config.Authority)
                            .WithHttpClientFactory(_httpFactory)
                            .Build());
            var result = await app
                    .AcquireTokenForClient([scope])
                    .ExecuteAsync();
            return result.AccessToken;
        }
        public TenantSetting GetClient(string type)
        {
            return type switch
            {
                "External" => _application.External,
                _ => _application.Corporate
            };
        }
        public string GetScope(string type)
        {
            return type switch
            {
                "External" => _security.ScopeExternal,
                _ => _security.ScopeCorporate
            };
        }
    }
}