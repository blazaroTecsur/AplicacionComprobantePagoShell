using Infor.Abstractions.DTOs;
using Infor.Abstractions.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Infor.Infrastructure.Services
{
    /// <summary>
    /// Cliente para el servicio IDO REST de Infor Syteline (SyteLineRESTv2).
    /// Base path: {BaseUrl}/{Tenant}/{AppId}/IDORequestService/ido/
    /// </summary>
    public sealed class InforIdoService : IInforIdoService
    {
        private readonly HttpClient _http;
        private readonly IInforTokenService _tokenService;
        private readonly InforSettings _settings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly ILogger<InforIdoService> _logger;

        private static readonly JsonSerializerOptions _jsonOpts = new()
        {
            PropertyNameCaseInsensitive = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public InforIdoService(
            HttpClient http,
            IInforTokenService tokenService,
            IOptions<InforSettings> settings,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            ILogger<InforIdoService> logger)
        {
            _http                = http;
            _tokenService        = tokenService;
            _settings            = settings.Value;
            _httpContextAccessor = httpContextAccessor;
            _configuration       = configuration;
            _logger              = logger;
        }

        // ── GET /load/{ido} — LoadCollection ─────────────────────────────────

        public async Task<JsonElement> LoadAsync(
            string ido,
            string? props = null,
            string? filter = null,
            int recordCap = 0,
            string? orderBy = null,
            CancellationToken ct = default)
        {
            var url = $"{_settings.IdoBaseUrl}load/{Uri.EscapeDataString(ido)}";

            var q = new List<string>();
            if (!string.IsNullOrWhiteSpace(props))   q.Add($"properties={Uri.EscapeDataString(props)}");
            if (!string.IsNullOrWhiteSpace(filter))  q.Add($"filter={Uri.EscapeDataString(filter).Replace("%27", "'")}");
            if (recordCap > 0)                       q.Add($"recordCap={recordCap}");
            if (!string.IsNullOrWhiteSpace(orderBy)) q.Add($"orderBy={Uri.EscapeDataString(orderBy)}");
            if (q.Count > 0) url += "?" + string.Join("&", q);

            return await EjecutarGetAsync(url, ct);
        }

        // ── GET /info/{ido} — IDOPropInfo ─────────────────────────────────────

        public async Task<JsonElement> IdoInfoAsync(
            string ido,
            CancellationToken ct = default)
        {
            var url = $"{_settings.IdoBaseUrl}info/{Uri.EscapeDataString(ido)}";
            return await EjecutarGetAsync(url, ct);
        }

        // ── POST /invoke/{ido}?method={method} — InvokeMethod ────────────────

        public async Task<JsonElement> InvokeMethodAsync(
            string ido,
            string method,
            IEnumerable<string>? parameters = null,
            CancellationToken ct = default)
        {
            var url  = $"{_settings.IdoBaseUrl}invoke/{Uri.EscapeDataString(ido)}?method={Uri.EscapeDataString(method)}";
            var body = parameters?.ToArray() ?? Array.Empty<string>();
            return await EjecutarPostAsync(url, body, ct);
        }

        // ── POST /update/{ido} — InsertItem (Action=1) ───────────────────────

        public async Task<JsonElement> InsertItemAsync(
            string ido,
            IEnumerable<IdoProperty> properties,
            bool refreshAfterSave = false,
            CancellationToken ct = default)
        {
            var body = new
            {
                IDOName = ido,
                Changes = new[]
                {
                    new
                    {
                        Action     = 1,
                        Properties = properties.ToList()
                    }
                }
            };

            var url = $"{_settings.IdoBaseUrl}update/{Uri.EscapeDataString(ido)}";
            if (refreshAfterSave) url += "?refresh=true";
            return await EjecutarPostAsync(url, body, ct);
        }

        // ── POST /update/{ido} — InsertItems (múltiples Changes) ─────────────

        public async Task<JsonElement> InsertItemsAsync(
            string ido,
            object payload,
            CancellationToken ct = default)
        {
            var url = $"{_settings.IdoBaseUrl}update/{Uri.EscapeDataString(ido)}";
            return await EjecutarPostAsync(url, payload, ct);
        }

        // ── POST /update/{ido} — UpdateItem (Action=2) ───────────────────────

        public async Task<JsonElement> UpdateItemAsync(
            string ido,
            object payload,
            CancellationToken ct = default)
        {
            var url = $"{_settings.IdoBaseUrl}update/{Uri.EscapeDataString(ido)}";
            return await EjecutarPostAsync(url, payload, ct);
        }

        // ── POST /update/{ido} — DeleteItem (Action=4) ───────────────────────

        public async Task<JsonElement> DeleteItemAsync(
            string ido,
            string itemId,
            CancellationToken ct = default)
        {
            var body = new
            {
                IDOName = ido,
                Changes = new[]
                {
                    new { Action = 4, ItemId = itemId }
                }
            };

            var url = $"{_settings.IdoBaseUrl}update/{Uri.EscapeDataString(ido)}";
            return await EjecutarPostAsync(url, body, ct);
        }

        // ── GET /configurations ───────────────────────────────────────────────

        public async Task<JsonElement> ObtenerConfiguracionesAsync(CancellationToken ct = default)
        {
            var url = $"{_settings.IdoBaseUrl}configurations";
            return await EjecutarGetAsync(url, ct);
        }

        // ── Helpers privados ──────────────────────────────────────────────────

        private async Task<JsonElement> EjecutarGetAsync(string url, CancellationToken ct)
        {
            _logger.LogInformation("IDO GET {Url}", url);
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            await AgregarAuthHeaderAsync(request);
            using var respuesta = await _http.SendAsync(request, ct);
            return await LeerRespuestaAsync(respuesta, ct);
        }

        private async Task<JsonElement> EjecutarPostAsync(string url, object? cuerpo, CancellationToken ct)
        {
            var json = JsonSerializer.Serialize(cuerpo, _jsonOpts);
            _logger.LogInformation("IDO POST {Url} | Payload: {Payload}", url, json);
            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            await AgregarAuthHeaderAsync(request);
            using var respuesta = await _http.SendAsync(request, ct);
            return await LeerRespuestaAsync(respuesta, ct);
        }

        private async Task AgregarAuthHeaderAsync(HttpRequestMessage request)
        {
            var token = await _tokenService.ObtenerTokenAsync();
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var config = ResolverConfiguracionMongoose();
            if (!string.IsNullOrWhiteSpace(config))
                request.Headers.TryAddWithoutValidation("X-Infor-MongooseConfig", config);
        }

        private string ResolverConfiguracionMongoose()
        {
            var user    = _httpContextAccessor.HttpContext?.User;
            var tid     = user?.FindFirst("tid")?.Value ?? string.Empty;
            var empresa = _configuration.GetSection("TenantEmpresas")[tid]
                          ?? user?.FindFirst("empresa")?.Value
                          ?? string.Empty;

            return _settings.Configuration.Replace("{EMPRESA}", empresa);
        }

        private static async Task<JsonElement> LeerRespuestaAsync(
            HttpResponseMessage respuesta, CancellationToken ct)
        {
            var contenido = await respuesta.Content.ReadAsStringAsync(ct);

            if (!respuesta.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Error en API Infor Syteline ({respuesta.StatusCode}): {contenido}");
            }

            var doc = JsonDocument.Parse(contenido).RootElement.Clone();

            // v2 usa Success:bool — false indica error de negocio
            if (doc.TryGetProperty("Success", out var success) &&
                success.ValueKind == JsonValueKind.False)
            {
                var msg = doc.TryGetProperty("Message", out var m) ? m.GetString() : contenido;
                throw new InvalidOperationException($"Syteline IDO error: {msg}");
            }

            return doc;
        }
    }
}
