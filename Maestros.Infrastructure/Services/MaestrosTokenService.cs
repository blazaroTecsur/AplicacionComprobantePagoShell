using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;

namespace Maestros.Infrastructure.Services
{
    public class MaestrosTokenService
    {
        private readonly MaestrosSettings _settings;
        private readonly IMsalHttpClientFactory _httpFactory;
        private IConfidentialClientApplication? _app;

        public MaestrosTokenService(
            IOptions<MaestrosSettings> settings,
            IMsalHttpClientFactory httpFactory)
        {
            _settings   = settings.Value;
            _httpFactory = httpFactory;
        }

        public async Task<string> GetTokenAsync()
        {
            _app ??= ConfidentialClientApplicationBuilder
                .Create(_settings.Auth.ClientId)
                .WithClientSecret(_settings.Auth.ClientSecret)
                .WithAuthority(_settings.Auth.Authority)
                .WithHttpClientFactory(_httpFactory)
                .Build();

            var result = await _app
                .AcquireTokenForClient(new[] { _settings.Auth.Scope })
                .ExecuteAsync();

            return result.AccessToken;
        }
    }
}
