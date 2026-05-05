using System.Net.Http.Json;
using ComprobantePago.Application.DTOs.Comprobante.Common;
using ComprobantePago.Application.Interfaces.Services.Maestros;
using ComprobantePago.Application.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ComprobantePago.Infrastructure.Services.Maestros
{
    public class ApiCatalogoUnidadService(
        HttpClient httpClient,
        IOptions<ApiMaestrosSettings> settings,
        ILogger<ApiCatalogoUnidadService> logger) : ICatalogoUnidadService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly ApiMaestrosSettings _settings = settings.Value;
        private readonly ILogger<ApiCatalogoUnidadService> _logger = logger;

        public async Task<IEnumerable<ComboDto>> ObtenerCodigosUnidadAsync(
            int unidad, string filtro = "")
        {
            try
            {
                var url = $"{_settings.BaseUrl}{_settings.Endpoints.CodigosUnidad}?unidad={unidad}&filtro={Uri.EscapeDataString(filtro)}";
                var result = await _httpClient.GetFromJsonAsync<IEnumerable<ComboDto>>(url);
                return result ?? Enumerable.Empty<ComboDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener códigos de unidad {Unidad} desde la API", unidad);
                return Enumerable.Empty<ComboDto>();
            }
        }
    }
}
