using Maestros.Abstractions.DTOs;
using Maestros.Abstractions.Interfaces;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace Maestros.Infrastructure.Services
{
    public sealed class MaestrosCatalogoUnidadService : MaestrosBaseService, IMaestrosCatalogoUnidadService
    {
        private readonly HttpClient _http;
        private readonly MaestrosSettings _settings;

        public MaestrosCatalogoUnidadService(
            HttpClient http,
            MaestrosTokenService tokenService,
            IOptions<MaestrosSettings> settings) : base(tokenService)
        {
            _http     = http;
            _settings = settings.Value;
        }

        public async Task<PagedResult<CodUnidadListDto>> GetByUnidadAsync(
            int unidad, string empresa, string? filtro, int pagina, int tamano,
            CancellationToken ct = default)
        {
            var url = $"{_settings.Endpoints.CodigosUnidad}{unidad}?pagina={pagina}&tamano={tamano}";
            if (!string.IsNullOrWhiteSpace(empresa))
                url += $"&empresa={Uri.EscapeDataString(empresa)}";
            if (!string.IsNullOrWhiteSpace(filtro))
                url += $"&filtro={Uri.EscapeDataString(filtro)}";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            await AgregarAuthHeaderAsync(request);

            using var response = await _http.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            var apiResponse = await response.Content
                .ReadFromJsonAsync<ApiResponse<PagedResult<CodUnidadListDto>>>(ct);

            return apiResponse?.Data ?? new PagedResult<CodUnidadListDto>();
        }
    }
}
