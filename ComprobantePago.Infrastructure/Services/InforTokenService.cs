using ComprobantePago.Application.Interfaces.Services;
using ComprobantePago.Application.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ComprobantePago.Infrastructure.Services
{
    /// <summary>
    /// Obtiene y cachea el token OAuth2 de Infor ION usando el flujo
    /// Resource Owner Password Credentials (cuenta de servicio).
    ///
    /// Formato correcto validado en Postman:
    ///   - client_id / client_secret → Authorization: Basic base64(ci:cs)
    ///   - username                  → {Tenant}#{saak}
    ///   - password                  → sask
    ///   - scope                     → Infor-ION
    /// </summary>
    public sealed class InforTokenService : IInforTokenService
    {
        private readonly HttpClient                _http;
        private readonly InforSettings             _settings;
        private readonly ILogger<InforTokenService> _logger;

        private string         _tokenCache  = string.Empty;
        private DateTime       _tokenExpira = DateTime.MinValue;
        private readonly SemaphoreSlim _lock = new(1, 1);

        public InforTokenService(
            HttpClient                 http,
            IOptions<InforSettings>    settings,
            ILogger<InforTokenService> logger)
        {
            _http     = http;
            _settings = settings.Value;
            _logger   = logger;
        }

        public async Task<string> ObtenerTokenAsync()
        {
            if (!string.IsNullOrEmpty(_tokenCache) && DateTime.UtcNow < _tokenExpira)
                return _tokenCache;

            await _lock.WaitAsync();
            try
            {
                if (!string.IsNullOrEmpty(_tokenCache) && DateTime.UtcNow < _tokenExpira)
                    return _tokenCache;

                _logger.LogInformation("Solicitando nuevo token a Infor ION...");

                // ── Body: grant + usuario de servicio ─────────────────────────
                // Username: {Tenant}#{saak}  (formato requerido por Infor)
                // Password: sask
                var body = new Dictionary<string, string>
                {
                    { "grant_type", "password" },
                    { "username",   _settings.ServiceAccountKey },
                    { "password",   _settings.ServiceAccountSecret },
                    { "scope",      "Infor-ION" }
                };

                // ── Header: Basic Auth con client_id:client_secret ────────────
                var credencial = Convert.ToBase64String(
                    Encoding.UTF8.GetBytes(
                        $"{_settings.ClientId}:{_settings.ClientSecret}"));

                using var request = new HttpRequestMessage(
                    HttpMethod.Post, _settings.TokenEndpoint)
                {
                    Content = new FormUrlEncodedContent(body)
                };
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Basic", credencial);

                using var respuesta = await _http.SendAsync(request);

                if (!respuesta.IsSuccessStatusCode)
                {
                    var cuerpo = await respuesta.Content.ReadAsStringAsync();
                    _logger.LogError(
                        "Error al obtener token Infor ION. Status: {Status} — {Body}",
                        respuesta.StatusCode, cuerpo);
                    throw new InvalidOperationException(
                        $"No se pudo obtener el token de Infor ION ({respuesta.StatusCode}): {cuerpo}");
                }

                var json = await respuesta.Content.ReadAsStringAsync();
                var doc  = JsonDocument.Parse(json).RootElement;

                _tokenCache = doc.GetProperty("access_token").GetString()
                    ?? throw new InvalidOperationException("La respuesta no contiene access_token.");

                var expiresIn = doc.TryGetProperty("expires_in", out var exp)
                    ? exp.GetInt32() : 3600;

                _tokenExpira = DateTime.UtcNow.AddSeconds(expiresIn - 60);

                _logger.LogInformation(
                    "Token Infor ION obtenido. Expira en {Segundos}s.", expiresIn - 60);

                return _tokenCache;
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}
