using ComprobantePago.Application.DTOs.Infor;
using ComprobantePago.Application.Interfaces.Services;
using ComprobantePago.Application.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace ComprobantePago.Infrastructure.Services
{
    /// <summary>
    /// Cliente para el servicio IDO REST de Infor Syteline (MGRestService.svc).
    /// URLs corregidas según la especificación real del servicio.
    /// </summary>
    public sealed class SytelineIdoService : ISytelineIdoService
    {
        private readonly HttpClient                  _http;
        private readonly IInforTokenService          _tokenService;
        private readonly InforSettings               _settings;
        private readonly ILogger<SytelineIdoService> _logger;

        private static readonly JsonSerializerOptions _jsonOpts = new()
        {
            PropertyNameCaseInsensitive = true,
            // Permite enviar caracteres como "ó", "á", etc. sin escapar a \uXXXX
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public SytelineIdoService(
            HttpClient                   http,
            IInforTokenService           tokenService,
            IOptions<InforSettings>      settings,
            ILogger<SytelineIdoService>  logger)
        {
            _http         = http;
            _tokenService = tokenService;
            _settings     = settings.Value;
            _logger       = logger;
        }

        // ── GET /json/{ido}[/{props}/adv] — LoadCollection ───────────────────
        // adv=true → props van en la URL (/json/{ido}/{props}/adv) y devuelve
        //            objetos con nombres de campo; adv=false → query params.

        public async Task<JsonElement> LoadAsync(
            string  ido,
            string? props     = null,
            string? filter    = null,
            int     recordCap = 0,
            string? orderBy   = null,
            bool    adv       = false,
            CancellationToken ct = default)
        {
            string url;
            if (adv && !string.IsNullOrWhiteSpace(props))
                url = $"{_settings.IdoBaseUrl}json/{Uri.EscapeDataString(ido)}/{Uri.EscapeDataString(props)}/adv";
            else
                url = $"{_settings.IdoBaseUrl}json/{Uri.EscapeDataString(ido)}";

            var q = new List<string>();
            if (!adv && !string.IsNullOrWhiteSpace(props)) q.Add($"props={Uri.EscapeDataString(props)}");
            if (!string.IsNullOrWhiteSpace(filter))        q.Add($"filter={Uri.EscapeDataString(filter)}");
            if (recordCap > 0)                             q.Add($"rowcap={recordCap}");
            if (!string.IsNullOrWhiteSpace(orderBy))       q.Add($"orderBy={Uri.EscapeDataString(orderBy)}");
            if (q.Count > 0) url += "?" + string.Join("&", q);

            _logger.LogDebug("IDO Load → {Url}", url);
            return await EjecutarGetAsync(url, ct);
        }

        // ── GET /json/idoinfo/{ido} — IDOPropInfo ────────────────────────────

        public async Task<JsonElement> IdoInfoAsync(
            string ido,
            CancellationToken ct = default)
        {
            var url = $"{_settings.IdoBaseUrl}json/idoinfo/{Uri.EscapeDataString(ido)}";
            _logger.LogDebug("IDO Info → {Url}", url);
            return await EjecutarGetAsync(url, ct);
        }

        // ── GET /json/method/{ido}/{method} — InvokeMethod ───────────────────

        public async Task<JsonElement> InvokeMethodAsync(
            string ido,
            string method,
            CancellationToken ct = default)
        {
            var url = $"{_settings.IdoBaseUrl}json/method/{Uri.EscapeDataString(ido)}/{Uri.EscapeDataString(method)}";
            _logger.LogDebug("IDO Method → {Url}", url);
            return await EjecutarGetAsync(url, ct);
        }

        // ── POST /json/{ido}/additem[/adv] — InsertItem ─────────────────────
        // Si se pasa refresh ("ALL" | "PROPS"), usa /additem/adv con los
        // query params refresh y props para recibir los campos actualizados.

        public async Task<JsonElement> InsertItemAsync(
            string ido,
            IEnumerable<IdoProperty> properties,
            string? refresh = null,
            string? props   = null,
            CancellationToken ct = default)
        {
            var guid      = Guid.NewGuid().ToString();
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

            var body = new
            {
                Action     = 1,
                ItemId     = $"PBT=[aptrx] apt.ID=[{guid}] apt.DT=[{timestamp}]",
                Properties = properties.ToList()
            };

            var endpoint = refresh is not null ? "additem/adv" : "additem";
            var url = $"{_settings.IdoBaseUrl}json/{Uri.EscapeDataString(ido)}/{endpoint}";

            if (refresh is not null || props is not null)
            {
                var q = new List<string>();
                if (refresh is not null) q.Add($"refresh={Uri.EscapeDataString(refresh)}");
                if (props   is not null) q.Add($"props={Uri.EscapeDataString(props)}");
                url += "?" + string.Join("&", q);
            }

            _logger.LogInformation("IDO AddItem → {Url}", url);
            return await EjecutarPostAsync(url, body, ct);
        }

        // ── POST /json/{ido}/additems — InsertItems ──────────────────────────

        public async Task<JsonElement> InsertItemsAsync(
            string ido,
            object payload,
            CancellationToken ct = default)
        {
            var url = $"{_settings.IdoBaseUrl}json/{Uri.EscapeDataString(ido)}/additems";
            _logger.LogDebug("IDO InsertItems → {Url}", url);
            return await EjecutarPostAsync(url, payload, ct);
        }

        // ── PUT /json/{ido}/updateitem — UpdateItem ──────────────────────────

        public async Task<JsonElement> UpdateItemAsync(
            string ido,
            object payload,
            CancellationToken ct = default)
        {
            var url = $"{_settings.IdoBaseUrl}json/{Uri.EscapeDataString(ido)}/updateitem";
            _logger.LogDebug("IDO UpdateItem → {Url}", url);
            return await EjecutarPutAsync(url, payload, ct);
        }

        // ── POST /json/{ido}/deleteitem — DeleteItem ──────────────────────────

        public async Task<JsonElement> DeleteItemAsync(
            string ido,
            string itemId,
            CancellationToken ct = default)
        {
            var url  = $"{_settings.IdoBaseUrl}json/{Uri.EscapeDataString(ido)}/deleteitem";
            var body = new { Action = 3, ItemId = itemId };
            _logger.LogInformation("IDO DeleteItem → {Url} | ItemId: {ItemId}", url, itemId);
            return await EjecutarPostAsync(url, body, ct);
        }

        // ── GET /json/configurations ──────────────────────────────────────────

        public async Task<JsonElement> ObtenerConfiguracionesAsync(CancellationToken ct = default)
        {
            var url = $"{_settings.IdoBaseUrl}json/configurations";
            _logger.LogDebug("IDO Configurations → {Url}", url);
            return await EjecutarGetAsync(url, ct);
        }

        // ── Helpers privados ──────────────────────────────────────────────────

        private async Task<JsonElement> EjecutarGetAsync(string url, CancellationToken ct)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            await AgregarAuthHeaderAsync(request);
            using var respuesta = await _http.SendAsync(request, ct);
            return await LeerRespuestaAsync(respuesta, ct);
        }

        private async Task<JsonElement> EjecutarPostAsync(string url, object? cuerpo, CancellationToken ct)
        {
            var json = JsonSerializer.Serialize(cuerpo, _jsonOpts);

            _logger.LogInformation("IDO POST → {Url} | Payload: {Payload}", url, json);

            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            await AgregarAuthHeaderAsync(request);
            using var respuesta = await _http.SendAsync(request, ct);
            return await LeerRespuestaAsync(respuesta, ct);
        }

        private async Task<JsonElement> EjecutarPutAsync(string url, object? cuerpo, CancellationToken ct)
        {
            using var request = new HttpRequestMessage(HttpMethod.Put, url)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(cuerpo, _jsonOpts),
                    Encoding.UTF8, "application/json")
            };
            await AgregarAuthHeaderAsync(request);
            using var respuesta = await _http.SendAsync(request, ct);
            return await LeerRespuestaAsync(respuesta, ct);
        }

        private async Task AgregarAuthHeaderAsync(HttpRequestMessage request)
        {
            var token = await _tokenService.ObtenerTokenAsync();
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Header obligatorio de Mongoose: identifica la configuración/BD de Syteline.
            // Sin él el IDO responde MessageCode 302 "Missing Mongoose configuration header".
            if (!string.IsNullOrWhiteSpace(_settings.Configuration))
                request.Headers.TryAddWithoutValidation("X-Infor-MongooseConfig", _settings.Configuration);
        }

        private async Task<JsonElement> LeerRespuestaAsync(
            HttpResponseMessage respuesta, CancellationToken ct)
        {
            var contenido = await respuesta.Content.ReadAsStringAsync(ct);

            _logger.LogInformation("IDO Response ({Status}): {Body}",
                (int)respuesta.StatusCode, contenido);

            if (!respuesta.IsSuccessStatusCode)
            {
                _logger.LogError("Error IDO REST. Status: {Status} — {Body}",
                    respuesta.StatusCode, contenido);
                throw new InvalidOperationException(
                    $"Error en API Infor Syteline ({respuesta.StatusCode}): {contenido}");
            }

            // Syteline usa dos códigos de éxito según la operación:
            //   0   → LoadCollection (GET)
            //   200 → InsertItem / UpdateItem (POST/PUT)
            // Cualquier otro código es error de negocio.
            var doc = JsonDocument.Parse(contenido).RootElement.Clone();
            if (doc.TryGetProperty("MessageCode", out var msgCode) &&
                msgCode.TryGetInt32(out var code) && code != 200 && code != 0)
            {
                var msg = doc.TryGetProperty("Message", out var m) ? m.GetString() : contenido;
                _logger.LogError("IDO operación fallida. MessageCode: {Code} — {Message}", code, msg);
                throw new InvalidOperationException(
                    $"Syteline IDO error {code}: {msg}");
            }

            return doc;
        }
    }
}
