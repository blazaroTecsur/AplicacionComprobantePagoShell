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
        private readonly SecuritySetting _settings;
        public SeguridadService(HttpClient httpClient, SeguridadTokenService tokenService, SecuritySetting settings)
        {
            _httpClient = httpClient;
            _tokenService = tokenService;
            _settings = settings;
        }
        public async Task<IEnumerable<SeguridadRolResponse>> ObtenerPermisos(
            string type, string codUsuario, string codApp)
        {
            var token = await _tokenService.GetTokenAsync(type);

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var query =
                new Dictionary<string, string>
                {
                    ["codUsuario"] = codUsuario,
                    ["codApp"] = codApp
                };

            var url = QueryHelpers.AddQueryString(_settings.Endpoints.ObtenerPermisos, query);
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