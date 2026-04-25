using Microsoft.Extensions.Configuration;
using Resguardo.Application.Common;
using Resguardo.Application.Queries.ConsultarCapataz;
using Resguardo.Application.Queries.ListarDepartamento;
using Resguardo.Application.Services;

namespace Resguardo.Infrastructure.Services
{
    public class MaestroService : IMaestroService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public MaestroService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }
        public async Task<GridResponse<ConsultarCapatazResponse>> ConsultarCapataz(GridRequest<ConsultarCapatazQuery> grid)
        {
            int total = 50;
            List<ConsultarCapatazResponse> capataces = new List<ConsultarCapatazResponse>();
            for (int i = 1; i <= total; i++)
            {
                capataces.Add(new ConsultarCapatazResponse
                {
                    CodCapataz = $"CAP{i.ToString().PadLeft(3, '0')}",
                    DniCapataz = "12345678",
                    NomCapataz = "Juan Pérez"
                });
            }

            return new GridResponse<ConsultarCapatazResponse>
            {
                Data = capataces,
                Total = total
            };          

            //var token = await _tokenAcquisition.GetAccessTokenForUserAsync(new[] { scope });
            //_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "toke_prueba");
            //var url = _configuration["ApiSettings:ObtenerSro"];
            //var response = await _httpClient.GetAsync(url);
            //response.EnsureSuccessStatusCode();
            //return await response.Content.ReadFromJsonAsync<CapatazDto>();
        }
        public async Task<IEnumerable<ListarDptoResponse>> ListarDepartamento()
        {
            int total = 15;
            List<ListarDptoResponse> dptos = new List<ListarDptoResponse>();
            for (int i = 1; i <= total; i++)
            {
                dptos.Add(new ListarDptoResponse
                {
                    Codigo = $"40{i.ToString().PadLeft(2, '0')}",
                    Nombre = $"DPTO OP {i.ToString().PadLeft(2, '0')}"
                });
            }

            return dptos;
        }
    }
}
