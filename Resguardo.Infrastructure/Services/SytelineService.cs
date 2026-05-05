using Microsoft.Extensions.Configuration;
using Resguardo.Application.Queries.ObtenerOrden;
using Resguardo.Application.Services;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Resguardo.Infrastructure.Services
{
    public class SytelineService : ISytelineService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public SytelineService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }
        public async Task<ObtenerOrdenResponse> ObtenerOrden(string codigo)
        {
            return new ObtenerOrdenResponse
            {
                CodSupr = "SUP001",
                NomSupr = "Juan Pérez",
                CodDpto = "4001",
                NomDpto = "Departamento de Construcción",
                RucSctta = "12345678901",
                NomSctta = "Constructora XYZ S.A.",
                FechaFoc = DateTime.Now.AddDays(-10),
                CodActv = "ACT001",
                NomActv = "Cambio de Postes",
                Estado = "EN EJECUCIÓN",
                Coordenada = "-12.1726108,-76.9724007",
                Direccion = "Pje. Calango 158, San Juan de Miraflores 15842"
            };

            //var token = await _tokenAcquisition.GetAccessTokenForUserAsync(new[] { scope });
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "toke_prueba");
            var url = _configuration["ApiSettings:ObtenerSro"];
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ObtenerOrdenResponse>();
        }
    }
}
