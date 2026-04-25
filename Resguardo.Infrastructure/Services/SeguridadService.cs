using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Resguardo.Application.DTOs.Response;
using Resguardo.Application.Exceptions;
using Resguardo.Application.Services;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Resguardo.Infrastructure.Services
{
    public class SeguridadService : ISeguridadService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly TokenService _token;        
        public SeguridadService(
            IConfiguration configuration, 
            HttpClient httpClient, 
            TokenService token,
            IMemoryCache cache)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _token = token;
        }
        public async Task<IEnumerable<SeguridadRolResponse>> ObtenerPermisos(string codTenant, string codUsuario, string codApp)
        {            
            var token = await _token.GetTokenAsync();            
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var url = _configuration["ApiSettings:Seguridad:ObtenerPermisos"];
            url = url + $"?codUsuario={codUsuario}&codTenant={codTenant}&codApp={codApp}";
            //var response = await _httpClient.GetAsync(url);
            //response.EnsureSuccessStatusCode();
            //return await response.Content.ReadFromJsonAsync<List<string>>();
            var response = await _httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<SeguridadResponse<IEnumerable<SeguridadRolResponse>>>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (apiResponse == null)
                throw new NotFoundException("PERMISOS");
            if (!apiResponse.Success)
                throw new BusinessException(StatusCodes.Status400BadRequest.ToString(), apiResponse.Error?.UserMessage);

            return apiResponse.Data;
        }
    }
}