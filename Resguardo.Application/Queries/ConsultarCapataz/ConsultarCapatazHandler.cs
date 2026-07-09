using Maestros.Abstractions.Interfaces;
using Resguardo.Application.Common;

namespace Resguardo.Application.Queries.ConsultarCapataz
{
    public class ConsultarCapatazHandler
    {
        private readonly IMaestrosSocioService _socio;
        public ConsultarCapatazHandler(IMaestrosSocioService socio)
        {
            _socio = socio;
        }
        public async Task<GridResponse<ConsultarCapatazResponse>> Ejecutar(GridRequest<ConsultarCapatazQuery> grid)
        {
            var capataces = new GridResponse<ConsultarCapatazResponse>();
            if (grid.Filtros is not null && !string.IsNullOrEmpty(grid.Filtros?.Proveedor))
            {
                var maestro = await _socio.GetAllAsync(grid.Filtros?.Proveedor, grid.Filtros?.Filtro, grid.Page, grid.PageSize);
                capataces = new GridResponse<ConsultarCapatazResponse>
                {
                    Data = maestro.Items.Select(s => new ConsultarCapatazResponse
                    {
                        CodCapataz = s.CodigoSocio,
                        NomCapataz = s.NombreCompleto,
                    }).ToList(),
                    Total = maestro.Total
                };
            }
            return capataces;
        }
    }
}