using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;

namespace Seguridad.Infrastructure.Services
{
    public class SeguridadTokenService
    {
        private readonly SeguridadSetting _settings;
        private readonly IConfidentialClientApplication _app;
        public SeguridadTokenService(
            IMsalHttpClientFactory httpClientFactory,
            IOptions<SeguridadSetting> settings)
        {
            _settings = settings.Value;
            _app = ConfidentialClientApplicationBuilder
                .Create(_settings.ClientId)
                .WithClientSecret(_settings.ClientSecret)
                .WithAuthority($"https://login.microsoftonline.com/{_settings.TenantId}")
                 .WithHttpClientFactory(httpClientFactory)
                .Build();
        }
        public async Task<string> GetTokenAsync()
        {
            var scope = _settings.Scope;
            var result = await _app
            .AcquireTokenForClient(new[] { scope })
            .ExecuteAsync();
            return result.AccessToken;
        }
    }
}