using Microsoft.Extensions.Caching.Memory;
using Microsoft.Identity.Web;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Shell.Web.Services
{
    public class PermisosService
    {
        private readonly ITokenAcquisition _tokenAcquisition;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;
        private static readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(10);

        public PermisosService(
            ITokenAcquisition tokenAcquisition,
            IConfiguration configuration,
            HttpClient httpClient,
            IMemoryCache cache)
        {
            _tokenAcquisition = tokenAcquisition;
            _configuration = configuration;
            _httpClient = httpClient;
            _cache = cache;
        }

        public async Task<IReadOnlyCollection<string>> ObtenerAsync(string codTenant, string codUsuario)
        {
            var cacheKey = $"permisos_{codTenant}_{codUsuario}";
            if (_cache.TryGetValue(cacheKey, out IReadOnlyCollection<string>? cached) && cached != null)
                return cached;

            var scope = _configuration["ApiSettings:Scope"]!;
            var token = await _tokenAcquisition.GetAccessTokenForUserAsync([scope]);

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var url = _configuration["ApiSettings:Permisos"]!;
            var codApp = _configuration["ApiSettings:CodAppComprobante"] ?? "COMPROBANTE";
            var fullUrl = $"{url}?codTenant={Uri.EscapeDataString(codTenant)}&codUsuario={Uri.EscapeDataString(codUsuario)}&codApp={Uri.EscapeDataString(codApp)}";

            var response = await _httpClient.GetAsync(fullUrl);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<IEnumerable<PermisoDto>>>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var permisos = (result?.Data?.Select(p => p.Codigo).ToList() ?? []) as IReadOnlyCollection<string>;

            _cache.Set(cacheKey, permisos, _cacheDuration);
            return permisos!;
        }

        public void Invalidar(string codTenant, string codUsuario)
        {
            _cache.Remove($"permisos_{codTenant}_{codUsuario}");
        }
    }
}
