using Resguardo.Application.Common;
using Resguardo.Application.Interfaces;
using Resguardo.Application.Queries.ConsultarServicio;

namespace Resguardo.Application.Queries.ConsultarSolicitud
{
    public class ConsultarServicioHandler
    {
        private readonly IServicioQueryService _query;
        public ConsultarServicioHandler(IServicioQueryService query)
        {
            _query = query;
        }
        public async Task<GridResponse<ConsultarServicioResponse>> Ejecutar(GridRequest<ConsultarServicioQuery> grid)
        {
            var servicios = await _query.Consultar(grid);
            return servicios;
        }
    }
}