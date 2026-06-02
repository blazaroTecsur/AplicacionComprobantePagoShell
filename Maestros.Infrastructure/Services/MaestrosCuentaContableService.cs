using Maestros.Abstractions.DTOs;
using Maestros.Abstractions.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace Maestros.Infrastructure.Services
{
    public sealed class MaestrosCuentaContableService : IMaestrosCuentaContableService
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _ctx;
        private readonly MaestrosSettings _settings;

        public MaestrosCuentaContableService(
            HttpClient http,
            IHttpContextAccessor ctx,
            IOptions<MaestrosSettings> settings)
        {
            _http     = http;
            _ctx      = ctx;
            _settings = settings.Value;
        }

        public async Task<PagedResult<CuentaContableListDto>> GetAllAsync(
            string? filtro, int pagina, int tamano, CancellationToken ct = default)
        {
            var url = $"{_settings.Endpoints.CuentasContables}?pagina={pagina}&tamano={tamano}";
            if (!string.IsNullOrWhiteSpace(filtro))
                url += $"&filtro={Uri.EscapeDataString(filtro)}";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            PropagateHeaders(request);

            using var response = await _http.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            var apiResponse = await response.Content
                .ReadFromJsonAsync<ApiResponse<PagedResult<CuentaContableListDto>>>(ct);

            return apiResponse?.Data ?? new PagedResult<CuentaContableListDto>();
        }

        private void PropagateHeaders(HttpRequestMessage request)
        {
            var headers = _ctx.HttpContext?.Request.Headers;
            if (headers is null) return;

            foreach (var key in new[]
            {
                "X-User-Oid", "X-Tenant-Id", "X-User-Email",
                "X-User-Name", "X-Session-Id", "X-Schema"
            })
            {
                if (headers.TryGetValue(key, out var val))
                    request.Headers.TryAddWithoutValidation(key, (string?)val);
            }
        }
    }
}
