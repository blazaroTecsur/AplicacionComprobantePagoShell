using Microsoft.AspNetCore.Authentication;
using Shell.Web.Models.ActualizarDatos;
using Shell.Web.Models.ObtenerUsuario;
using System.Net.Http.Headers;
using System.Text;
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
        public async Task<ApiResponse<string>> ActualizarDatos(ActualizarViewModel usuario)
        {
            var context = _httpContextAccessor.HttpContext;
            var token = await context.GetTokenAsync("access_token");

            if (string.IsNullOrWhiteSpace(token))
                throw new Exception("No se encontró access_token");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var url = _configuration["ApiSettings:Actualizar"];
            var json = JsonSerializer.Serialize(usuario);
            using var contentJson = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(url, contentJson);
            var content = await response.Content.ReadAsStringAsync();
            
            if (string.IsNullOrWhiteSpace(content))
            {
                return ApiResponse<string>.Fail(
                    new ApiErrorDetail
                    {
                        Code = "EMPTY_RESPONSE",
                        UserMessage = "El servidor no devolvió información."
                    });
            }
            ApiResponse<string>? apiResponse;
            try
            {
                apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(
                    content,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
            }
            catch
            {
                return ApiResponse<string>.Fail(
                    new ApiErrorDetail
                    {
                        Code = "INVALID_JSON",
                        UserMessage = "La respuesta del servidor no tiene un formato válido."
                    });
            }

            return apiResponse;
        }
    }
}