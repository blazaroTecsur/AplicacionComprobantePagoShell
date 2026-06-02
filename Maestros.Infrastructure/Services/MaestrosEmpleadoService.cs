using Maestros.Abstractions.DTOs;
using Maestros.Abstractions.Interfaces;
using Microsoft.AspNetCore.Http;
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
            IHttpContextAccessor ctx,
            IOptions<MaestrosSettings> settings) : base(ctx)
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
            PropagateHeaders(request);

            using var response = await _http.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            var apiResponse = await response.Content
                .ReadFromJsonAsync<ApiResponse<PagedResult<EmpleadoListDto>>>(ct);

            return apiResponse?.Data ?? new PagedResult<EmpleadoListDto>();
        }
    }
}
