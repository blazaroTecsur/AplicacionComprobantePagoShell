using Maestros.Abstractions.DTOs;
using Maestros.Abstractions.Interfaces;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace Maestros.Infrastructure.Services
{
    public sealed class MaestrosEmpleadoService : MaestrosBaseService, IMaestrosEmpleadoService
    {
        private readonly HttpClient _http;
        private readonly MaestrosSettings _settings;

        public MaestrosEmpleadoService(
            HttpClient http,
            MaestrosTokenService tokenService,
            IOptions<MaestrosSettings> settings) : base(tokenService)
        {
            _http     = http;
            _settings = settings.Value;
        }

        public async Task<PagedResult<EmpleadoListDto>> GetAllAsync(
            string? filtro, int pagina, int tamano, CancellationToken ct = default)
        {
            var url = $"{_settings.Endpoints.Empleados}?pagina={pagina}&tamano={tamano}";
            if (!string.IsNullOrWhiteSpace(filtro))
                url += $"&filtro={Uri.EscapeDataString(filtro)}";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            await AgregarAuthHeaderAsync(request);

            using var response = await _http.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            var apiResponse = await response.Content
                .ReadFromJsonAsync<ApiResponse<PagedResult<EmpleadoListDto>>>(ct);

            return apiResponse?.Data ?? new PagedResult<EmpleadoListDto>();
        }
    }
}
