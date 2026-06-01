using System.Net.Http.Json;
using System.Text.Json;
using ComprobantePago.Application.DTOs.Comprobante.Common;
using ComprobantePago.Application.Interfaces.Services.Maestros;
using ComprobantePago.Application.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ComprobantePago.Infrastructure.Services.Maestros
{
    public class ApiCuentaContableService(
        HttpClient httpClient,
        IOptions<ApiMaestrosSettings> settings,
        IHttpContextAccessor httpContextAccessor,
        ILogger<ApiCuentaContableService> logger) : ICuentaContableService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly ApiMaestrosSettings _settings = settings.Value;
        private readonly IHttpContextAccessor _ctx = httpContextAccessor;
        private readonly ILogger<ApiCuentaContableService> _logger = logger;

        private static readonly JsonSerializerOptions _jsonOpts = new() { PropertyNameCaseInsensitive = true };

        public async Task<IEnumerable<ComboDto>> ObtenerCuentasContablesAsync(string filtro = "")
        {
            try
            {
                var url = $"{_settings.BaseUrl.TrimEnd('/')}/{_settings.Endpoints.CuentasContables.TrimStart('/')}?tamano=500";
                if (!string.IsNullOrWhiteSpace(filtro)) url += $"&filtro={Uri.EscapeDataString(filtro)}";

                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                PropagateHeaders(request);

                using var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var wrapper = await response.Content
                    .ReadFromJsonAsync<MaestrosResponse<PagedData<CuentaContableItem>>>(_jsonOpts);

                return wrapper?.Data?.Items
                    .Where(c => c.Cuenta.Length >= 7)
                    .Select(c => new ComboDto
                    {
                        Codigo      = c.Cuenta,
                        Descripcion = c.Descripcion,
                    }) ?? Enumerable.Empty<ComboDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cuentas contables desde la API");
                return Enumerable.Empty<ComboDto>();
            }
        }

        private void PropagateHeaders(HttpRequestMessage request)
        {
            var headers = _ctx.HttpContext?.Request.Headers;
            if (headers is null) return;
            foreach (var key in new[] { "X-User-Oid", "X-Tenant-Id", "X-User-Email",
                                         "X-User-Name", "X-Session-Id", "X-Schema" })
            {
                if (headers.TryGetValue(key, out var val))
                    request.Headers.TryAddWithoutValidation(key, (string?)val);
            }
        }

        private record MaestrosResponse<T>(bool Exito, T? Data, string Mensaje);
        private record PagedData<T>(IEnumerable<T> Items, int Total, int Pagina, int Tamano);
        private record CuentaContableItem(string Cuenta, string Descripcion);
    }
}
