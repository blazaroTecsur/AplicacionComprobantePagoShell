using System.Net.Http.Json;
using ComprobantePago.Application.DTOs.Comprobante.Common;
using ComprobantePago.Application.Interfaces.Services.Maestros;
using ComprobantePago.Application.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ComprobantePago.Infrastructure.Services.Maestros
{
    public class ApiProveedorService(
        HttpClient httpClient,
        IOptions<ApiMaestrosSettings> settings,
        ILogger<ApiProveedorService> logger) : IProveedorService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly ApiMaestrosSettings _settings = settings.Value;
        private readonly ILogger<ApiProveedorService> _logger = logger;

        public async Task<IEnumerable<ComboDto>> ObtenerProveedoresAsync(string filtro = "")
        {
            try
            {
                var url = $"{_settings.BaseUrl}{_settings.Endpoints.Proveedores}?filtro={Uri.EscapeDataString(filtro)}";
                var result = await _httpClient.GetFromJsonAsync<IEnumerable<ComboDto>>(url);
                return result ?? Enumerable.Empty<ComboDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener proveedores desde la API");
                return Enumerable.Empty<ComboDto>();
            }
        }
    }
}
