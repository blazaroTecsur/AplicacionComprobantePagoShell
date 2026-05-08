using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using System.Collections.Concurrent;

namespace Seguridad.Infrastructure.Services
{
    public class SeguridadTokenService
    {
        private readonly SeguridadSetting _settings;
        private readonly IMsalHttpClientFactory _httpFactory;
        private readonly ConcurrentDictionary<string, IConfidentialClientApplication> _clients = new();
        public SeguridadTokenService(
            IMsalHttpClientFactory httpFactory,
            IOptions<SeguridadSetting> settings)
        {
            _settings = settings.Value;
            _httpFactory = httpFactory;
        }
        public async Task<string> GetTokenAsync(string type)
        {
            var config = GetConfiguration(type);
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
                    .AcquireTokenForClient(new[] { config.Scope })
                    .ExecuteAsync();
            return result.AccessToken;
        }
        public TenantSetting GetConfiguration(string type)
        {
            return type switch
            {
                "External" => _settings.External,
                _ => _settings.Corporate
            };
        }
    }
}