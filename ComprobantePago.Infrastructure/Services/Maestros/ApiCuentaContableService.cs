using System.Net.Http.Json;
using ComprobantePago.Application.DTOs.Comprobante.Common;
using ComprobantePago.Application.Interfaces.Services.Maestros;
using ComprobantePago.Application.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Seguridad.Abstractions.Interfaces;

namespace ComprobantePago.Infrastructure.Services.Maestros
{
    public class ApiCuentaContableService(
        HttpClient httpClient,
        IOptions<ApiMaestrosSettings> settings,
        ILogger<ApiCuentaContableService> logger,
        IUsuarioContexto usuario) : ICuentaContableService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly ApiMaestrosSettings _settings = settings.Value;
        private readonly ILogger<ApiCuentaContableService> _logger = logger;
        private readonly IUsuarioContexto _usuario = usuario;

        public async Task<IEnumerable<ComboDto>> ObtenerCuentasContablesAsync(string filtro = "")
        {
            try
            {
                var empresa = Uri.EscapeDataString(_usuario.Empresa ?? "");
                var url = $"{_settings.BaseUrl}{_settings.Endpoints.CuentasContables}?empresa={empresa}&filtro={Uri.EscapeDataString(filtro)}";
                var result = await _httpClient.GetFromJsonAsync<IEnumerable<ComboDto>>(url);
                return result ?? Enumerable.Empty<ComboDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cuentas contables desde la API");
                return Enumerable.Empty<ComboDto>();
            }
        }
    }
}
