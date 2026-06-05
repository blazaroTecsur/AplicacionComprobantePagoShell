using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;

namespace Maestros.Infrastructure.Services
{
    public class MaestrosTokenService
    {
        private readonly MaestrosSettings _settings;
        private readonly IMsalHttpClientFactory _httpFactory;
        private readonly ILogger<MaestrosTokenService> _logger;
        private IConfidentialClientApplication? _app;

        public MaestrosTokenService(
            IOptions<MaestrosSettings> settings,
            IMsalHttpClientFactory httpFactory,
            ILogger<MaestrosTokenService> logger)
        {
            _settings    = settings.Value;
            _httpFactory = httpFactory;
            _logger      = logger;
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

            _logger.LogInformation(
                "Maestros token obtenido — ClientId: {ClientId} Scope: {Scope} ExpiresOn: {Expiry} FromCache: {Cache}",
                _settings.Auth.ClientId, _settings.Auth.Scope,
                result.ExpiresOn, result.AuthenticationResultMetadata.TokenSource);

            return result.AccessToken;
        }
    }
}
