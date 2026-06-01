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
    public class ApiEmpleadoService(
        HttpClient httpClient,
        IOptions<ApiMaestrosSettings> settings,
        IHttpContextAccessor httpContextAccessor,
        ILogger<ApiEmpleadoService> logger) : IEmpleadoService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly ApiMaestrosSettings _settings = settings.Value;
        private readonly IHttpContextAccessor _ctx = httpContextAccessor;
        private readonly ILogger<ApiEmpleadoService> _logger = logger;

        private static readonly JsonSerializerOptions _jsonOpts = new() { PropertyNameCaseInsensitive = true };

        public async Task<IEnumerable<ComboDto>> ObtenerEmpleadosAsync(string filtro = "")
        {
            try
            {
                var url = $"{_settings.BaseUrl.TrimEnd('/')}/{_settings.Endpoints.Empleados.TrimStart('/')}?tamano=500";
                if (!string.IsNullOrWhiteSpace(filtro)) url += $"&filtro={Uri.EscapeDataString(filtro)}";

                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                PropagateHeaders(request);

                using var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var wrapper = await response.Content
                    .ReadFromJsonAsync<MaestrosResponse<PagedData<EmpleadoItem>>>(_jsonOpts);

                return wrapper?.Data?.Items.Select(e => new ComboDto
                {
                    Codigo      = e.Codigo,
                    Descripcion = e.NombreCompleto,
                }) ?? Enumerable.Empty<ComboDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener empleados desde la API");
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
        private record EmpleadoItem(string Codigo, string NombreCompleto);
    }
}
