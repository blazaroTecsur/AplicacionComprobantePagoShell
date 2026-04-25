using Resguardo.Application.Common;
using Resguardo.Application.Interfaces;

namespace Resguardo.Application.Queries.ConsultarSolicitud
{
    public class ConsultarSolicitudHandler
    {
        private readonly ISolicitudQueryService _query;
        public ConsultarSolicitudHandler(ISolicitudQueryService query)
        {
            _query = query;
        }
        public async Task<GridResponse<ConsultarSolicitudResponse>> Ejecutar(GridRequest<ConsultarSolicitudQuery> grid)
        {
            var solicitudes = await _query.Consultar(grid);
            return solicitudes;
        }
    }
}
