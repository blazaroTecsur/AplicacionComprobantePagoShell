using Microsoft.Identity.Web;
using Shell.Web.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Shell.Web.Services
{
    public class ApiService
    {
        private readonly ITokenAcquisition _tokenAcquisition;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly PermisosService _permisosService;

        public ApiService(
            ITokenAcquisition tokenAcquisition,
            IConfiguration configuration,
            HttpClient httpClient,
            PermisosService permisosService)
        {
            _tokenAcquisition = tokenAcquisition;
            _configuration = configuration;
            _httpClient = httpClient;
            _permisosService = permisosService;
        }

        public async Task<UsuarioViewModel> ObtenerUsuario()
        {
            var scope = _configuration["ApiSettings:Scope"];
            var token = await _tokenAcquisition.GetAccessTokenForUserAsync([scope]);

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var url = _configuration["ApiSettings:Autenticar"];
            var response = await _httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<UsuarioViewModel>>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (apiResponse == null)
                throw new Exception("Respuesta inválida");
            if (!apiResponse.Success)
                throw new Exception(apiResponse.Error?.UserMessage);

            var usuario = apiResponse.Data;
            var permisos = await _permisosService.ObtenerAsync(usuario.CodTenant, usuario.CodUsuario);
            usuario.Permisos = permisos;

            return usuario;
        }
    }
}