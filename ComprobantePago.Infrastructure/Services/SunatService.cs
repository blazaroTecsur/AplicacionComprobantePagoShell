using ComprobantePago.Application.Common;
using ComprobantePago.Application.DTOs.Comprobante.Response;
using ComprobantePago.Application.Interfaces.Services;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ComprobantePago.Infrastructure.Services
{
    public class SunatService : ISunatService
    {
        private readonly HttpClient _httpClient;
        private readonly SunatSettings _settings;
        private string _tokenCache = string.Empty;
        private DateTime _tokenExpira = DateTime.MinValue;

        public SunatService(
            HttpClient httpClient,
            IOptions<SunatSettings> settings)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
        }

        // ── Obtener Token OAuth2 ──────────────────
        private async Task<string> ObtenerTokenAsync()
        {
            // Reutilizar token si aún es válido
            if (!string.IsNullOrEmpty(_tokenCache) &&
                DateTime.Now < _tokenExpira)
                return _tokenCache;

            var tokenUrl = _settings.TokenUrl
                .Replace("{client_id}", _settings.ClientId);

            var parametros = new Dictionary<string, string>
            {
                { "grant_type",    "client_credentials" },
                { "scope",         _settings.Scope },
                { "client_id",     _settings.ClientId },
                { "client_secret", _settings.ClientSecret }
            };

            var response = await _httpClient.PostAsync(
                tokenUrl,
                new FormUrlEncodedContent(parametros)
            );

            if (!response.IsSuccessStatusCode)
                throw new Exception("Error al obtener token SUNAT.");

            var json = await response.Content.ReadAsStringAsync();
            var resultado = JsonSerializer.Deserialize<JsonElement>(json);

            _tokenCache = resultado
                .GetProperty("access_token")
                .GetString() ?? string.Empty;

            _tokenExpira = DateTime.Now.AddSeconds(
                resultado.GetProperty("expires_in").GetInt32() - 60
            );

            return _tokenCache;
        }

        // ── Validar comprobante ───────────────────
        public async Task<ValidacionSunatDto> ValidarComprobanteAsync(
            string numRuc,
            string codComp,
            string numeroSerie,
            string numero,
            string fechaEmision,
            decimal monto)
        {
            try
            {
                var token = await ObtenerTokenAsync();
                var url = $"{_settings.ApiUrl}/{_settings.RucEmpresa}" +
                          "/validarcomprobante";

                var body = new
                {
                    numRuc,
                    codComp,
                    numeroSerie,
                    numero,
                    fechaEmision,
                    monto
                };

                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(
                        JsonSerializer.Serialize(body),
                        Encoding.UTF8,
                        "application/json"
                    )
                };

                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();
                var resultado = JsonSerializer.Deserialize<JsonElement>(json);

                if (!response.IsSuccessStatusCode)
                {
                    return new ValidacionSunatDto
                    {
                        Exito        = false,
                        EstadoSunat  = "ERROR",
                        CodigoEstado = "ERROR",
                        Motivo       = "Error al conectar con SUNAT."
                    };
                }

                var success = resultado
                    .GetProperty("success").GetBoolean();
                var data = resultado.GetProperty("data");
                var estadoCp = data
                    .GetProperty("estadoCp")
                    .GetString() ?? "0";

                // Mapear código a descripción
                var estadoDescripcion = estadoCp switch
                {
                    "0" => "NO EXISTE", 
                    "1" => "ACEPTADO",
                    "2" => "ANULADO",
                    "3" => "AUTORIZADO",
                    "4" => "NO AUTORIZADO",
                    _ => "DESCONOCIDO"
                };

                return new ValidacionSunatDto
                {
                    Exito = success,
                    EstadoSunat = estadoDescripcion,
                    CodigoEstado = estadoCp,
                    Motivo = resultado
                    .GetProperty("message")
                    .GetString() ?? string.Empty
                };
            }
            catch (Exception ex)
            {
                return new ValidacionSunatDto
                {
                    Exito        = false,
                    EstadoSunat  = "ERROR",
                    CodigoEstado = "ERROR",
                    Motivo       = ex.Message
                };
            }
        }
    }
}
