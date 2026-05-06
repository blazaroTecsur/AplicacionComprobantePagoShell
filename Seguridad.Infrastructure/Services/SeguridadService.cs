using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Seguridad.Abstractions.DTOs;
using Seguridad.Abstractions.Interfaces;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Seguridad.Infrastructure.Services
{
    public class SeguridadService : ISeguridadService
    {
        private readonly SeguridadSetting _settings;
        private readonly HttpClient _httpClient;
        private readonly SeguridadTokenService _token;
        public SeguridadService(
            HttpClient httpClient,
            SeguridadTokenService token,
            IOptions<SeguridadSetting> settings)
        {
            _httpClient = httpClient;
            _token = token;
            _settings = settings.Value;
        }
        public async Task<IEnumerable<SeguridadRolResponse>> ObtenerPermisos(
            string codTenant, string codUsuario, string codApp)
        {
            var token = await _token.GetTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var query = new Dictionary<string, string>
            {
                ["codUsuario"] = codUsuario,
                ["codTenant"] = codTenant,
                ["codApp"] = codApp
            };
            var url = QueryHelpers.AddQueryString(_settings.ObtenerPermisos, query);
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Error HTTP: {response.StatusCode}");
            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<SeguridadResponse<IEnumerable<SeguridadRolResponse>>>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (apiResponse == null || !apiResponse.Success)
                throw new UnauthorizedAccessException($"No tiene permisos para acceder a la aplicación de {codApp}");

            return apiResponse.Data;
        }
    }
}