using Microsoft.AspNetCore.WebUtilities;
using Seguridad.Abstractions.DTOs;
using Seguridad.Abstractions.Interfaces;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Seguridad.Infrastructure.Services
{
    public class SeguridadService : ISeguridadService
    {
        private readonly HttpClient _httpClient;
        private readonly SeguridadTokenService _tokenService;
        public SeguridadService(HttpClient httpClient, SeguridadTokenService tokenService)
        {
            _httpClient = httpClient;
            _tokenService = tokenService;
        }
        public async Task<IEnumerable<SeguridadRolResponse>> ObtenerPermisos(
            string type, string codTenant, string codUsuario, string codApp)
        {
            var token = await _tokenService.GetTokenAsync(type);
            var config = _tokenService.GetConfiguration(type);

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var query =
                new Dictionary<string, string>
                {
                    ["codUsuario"] = codUsuario,
                    ["codTenant"] = codTenant,
                    ["codApp"] = codApp
                };

            var url = QueryHelpers.AddQueryString(config.ObtenerPermisos, query);
            var response = await _httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw new Exception($"HTTP {(int)response.StatusCode}: {content}");

            var apiResponse =
                JsonSerializer.Deserialize<
                    SeguridadResponse<
                        IEnumerable<SeguridadRolResponse>>>(
                    content,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            if (apiResponse == null || !apiResponse.Success)
                throw new UnauthorizedAccessException($"No tiene permisos para acceder a {codApp}");
            return apiResponse.Data;
        }
    }
}