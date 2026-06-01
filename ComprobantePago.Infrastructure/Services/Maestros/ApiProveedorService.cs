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
    public class ApiProveedorService(
        HttpClient httpClient,
        IOptions<ApiMaestrosSettings> settings,
        IHttpContextAccessor httpContextAccessor,
        ILogger<ApiProveedorService> logger) : IProveedorService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly ApiMaestrosSettings _settings = settings.Value;
        private readonly IHttpContextAccessor _ctx = httpContextAccessor;
        private readonly ILogger<ApiProveedorService> _logger = logger;

        private static readonly JsonSerializerOptions _jsonOpts = new() { PropertyNameCaseInsensitive = true };

        public async Task<IEnumerable<ComboDto>> ObtenerProveedoresAsync(string filtro = "")
        {
            try
            {
                var url = $"{_settings.BaseUrl.TrimEnd('/')}/{_settings.Endpoints.Proveedores.TrimStart('/')}?filtro={Uri.EscapeDataString(filtro)}&tamano=100";

                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                PropagateHeaders(request);

                using var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var wrapper = await response.Content
                    .ReadFromJsonAsync<MaestrosResponse<PagedData<ProveedorItem>>>(_jsonOpts);

                return wrapper?.Data?.Items.Select(p => new ComboDto
                {
                    Codigo      = p.Ruc,
                    Descripcion = p.NombreProveedor,
                }) ?? Enumerable.Empty<ComboDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener proveedores desde la API");
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
        private record ProveedorItem(string Ruc, string IdProveedorExternal, string NombreProveedor, string Estado);
    }
}
