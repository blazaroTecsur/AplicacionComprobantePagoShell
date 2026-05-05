using System.Net.Http.Json;
using ComprobantePago.Application.DTOs.Comprobante.Common;
using ComprobantePago.Application.Interfaces.Services.Maestros;
using ComprobantePago.Application.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ComprobantePago.Infrastructure.Services.Maestros
{
    public class ApiEmpleadoService(
        HttpClient httpClient,
        IOptions<ApiMaestrosSettings> settings,
        ILogger<ApiEmpleadoService> logger) : IEmpleadoService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly ApiMaestrosSettings _settings = settings.Value;
        private readonly ILogger<ApiEmpleadoService> _logger = logger;

        public async Task<IEnumerable<ComboDto>> ObtenerEmpleadosAsync(string filtro = "")
        {
            try
            {
                var url = $"{_settings.BaseUrl}{_settings.Endpoints.Empleados}?filtro={Uri.EscapeDataString(filtro)}";
                var result = await _httpClient.GetFromJsonAsync<IEnumerable<ComboDto>>(url);
                return result ?? Enumerable.Empty<ComboDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener empleados desde la API");
                return Enumerable.Empty<ComboDto>();
            }
        }
    }
}
