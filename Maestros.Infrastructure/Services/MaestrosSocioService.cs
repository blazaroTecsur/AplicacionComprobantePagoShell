using Maestros.Abstractions.DTOs;
using Maestros.Abstractions.Interfaces;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Json;

namespace Maestros.Infrastructure.Services
{
    public sealed class MaestrosSocioService : MaestrosBaseService, IMaestrosSocioService
    {
        private readonly HttpClient _http;
        private readonly MaestrosSettings _settings;

        public MaestrosSocioService(
            HttpClient http,
            MaestrosTokenService tokenService,
            IOptions<MaestrosSettings> settings) : base(tokenService)
        {
            _http     = http;
            _settings = settings.Value;
        }

        public async Task<PagedResult<SocioListDto>> GetAllAsync(
            string? proveedor, string? filtro, int pagina, int tamano, CancellationToken ct = default)
        {
            var url = $"{_settings.Endpoints.Socios}?pagina={pagina}&tamano={tamano}";
            if (!string.IsNullOrWhiteSpace(proveedor))
                url += $"&proveedor={Uri.EscapeDataString(proveedor)}";
            if (!string.IsNullOrWhiteSpace(filtro))
                url += $"&filtro={Uri.EscapeDataString(filtro)}";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            await AgregarAuthHeaderAsync(request);

            using var response = await _http.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            var apiResponse = await response.Content
                .ReadFromJsonAsync<ApiResponse<PagedResult<SocioListDto>>>(ct);

            return apiResponse?.Data ?? new PagedResult<SocioListDto>();
        }

        public async Task<SocioListDto?> GetByCodigoAsync(
            string codigo, CancellationToken ct = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get,
                $"{_settings.Endpoints.Socios}/{Uri.EscapeDataString(codigo)}");
            await AgregarAuthHeaderAsync(request);

            using var response = await _http.SendAsync(request, ct);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();

            var apiResponse = await response.Content
                .ReadFromJsonAsync<ApiResponse<SocioListDto>>(ct);

            return apiResponse?.Data;
        }
    }
}
