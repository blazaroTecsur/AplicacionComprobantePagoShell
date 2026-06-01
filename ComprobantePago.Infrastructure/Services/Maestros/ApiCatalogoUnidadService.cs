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
    public class ApiCatalogoUnidadService(
        HttpClient httpClient,
        IOptions<ApiMaestrosSettings> settings,
        IHttpContextAccessor httpContextAccessor,
        ILogger<ApiCatalogoUnidadService> logger) : ICatalogoUnidadService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly ApiMaestrosSettings _settings = settings.Value;
        private readonly IHttpContextAccessor _ctx = httpContextAccessor;
        private readonly ILogger<ApiCatalogoUnidadService> _logger = logger;

        private static readonly JsonSerializerOptions _jsonOpts = new() { PropertyNameCaseInsensitive = true };

        public async Task<IEnumerable<ComboDto>> ObtenerCodigosUnidadAsync(int unidad, string filtro = "")
        {
            try
            {
                var endpoint = $"{_settings.BaseUrl.TrimEnd('/')}/api/v1/cods-unidad{unidad}";
                var empresa  = _ctx.HttpContext?.Request.Headers["X-Schema"].ToString();

                var query = "?tamano=500";
                if (!string.IsNullOrWhiteSpace(empresa)) query += $"&empresa={Uri.EscapeDataString(empresa)}";
                if (!string.IsNullOrWhiteSpace(filtro))  query += $"&filtro={Uri.EscapeDataString(filtro)}";

                using var request = new HttpRequestMessage(HttpMethod.Get, endpoint + query);
                PropagateHeaders(request);

                using var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var wrapper = await response.Content
                    .ReadFromJsonAsync<MaestrosResponse<PagedData<CodUnidadItem>>>(_jsonOpts);

                return wrapper?.Data?.Items.Select(c => new ComboDto
                {
                    Codigo      = c.Codigo,
                    Descripcion = c.Descripcion,
                }) ?? Enumerable.Empty<ComboDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener códigos de unidad {Unidad} desde la API", unidad);
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
        private record CodUnidadItem(string Codigo, string Descripcion);
    }
}
