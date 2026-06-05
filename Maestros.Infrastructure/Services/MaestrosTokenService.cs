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
            if (string.IsNullOrWhiteSpace(_settings.Auth.ClientId) ||
                string.IsNullOrWhiteSpace(_settings.Auth.ClientSecret))
            {
                _logger.LogError(
                    "Maestros Auth no configurado — ClientId o ClientSecret vacíos en ApiMaestros:Auth");
                throw new InvalidOperationException(
                    "ApiMaestros:Auth:ClientId y ClientSecret deben estar configurados.");
            }

            _app ??= ConfidentialClientApplicationBuilder
                .Create(_settings.Auth.ClientId)
                .WithClientSecret(_settings.Auth.ClientSecret)
                .WithAuthority(_settings.Auth.Authority)
                .WithHttpClientFactory(_httpFactory)
                .Build();

            try
            {
                var result = await _app
                    .AcquireTokenForClient(new[] { _settings.Auth.Scope })
                    .ExecuteAsync();

                _logger.LogInformation(
                    "Maestros token obtenido — ClientId: {ClientId} Scope: {Scope} ExpiresOn: {Expiry} FromCache: {Cache}",
                    _settings.Auth.ClientId, _settings.Auth.Scope,
                    result.ExpiresOn, result.AuthenticationResultMetadata.TokenSource);

                return result.AccessToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error al obtener token de Maestros — ClientId: {ClientId} Authority: {Authority} Scope: {Scope}",
                    _settings.Auth.ClientId, _settings.Auth.Authority, _settings.Auth.Scope);
                throw;
            }
        }
    }
}
