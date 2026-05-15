using Microsoft.AspNetCore.Authentication;
using Shell.Web.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Shell.Web.Services
{
    public class ApiService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public ApiService(
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            HttpClient httpClient)
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task<UsuarioViewModel> ObtenerUsuario()
        {
            var context = _httpContextAccessor.HttpContext;
            var token = await context.GetTokenAsync("access_token");

            if (string.IsNullOrWhiteSpace(token))
                throw new Exception("No se encontró access_token");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var url = _configuration["ApiSettings:Autenticar"];
            var response = await _httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"API ERROR: {(int)response.StatusCode} - {content}");
            if (string.IsNullOrWhiteSpace(content))
                throw new Exception("API retornó contenido vacío");

            var apiResponse = JsonSerializer.Deserialize<ApiResponse<UsuarioViewModel>>(content,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            if (apiResponse == null)
                throw new Exception("Respuesta inválida");
            if (!apiResponse.Success)
                throw new Exception(apiResponse.Error?.UserMessage);

            return apiResponse.Data;
        }
    }
}