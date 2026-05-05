using Resguardo.Application.Common;
using Resguardo.Application.Queries.ConsultarSolicitud;
using Resguardo.Application.Queries.ObtenerSolicitud;
using Resguardo.Application.Queries.ReporteSolicitud;

namespace Resguardo.Application.Interfaces
{
    public interface ISolicitudQueryService
    {
        Task<ObtenerSolicitudResponse?> Obtener(int id);
        Task<GridResponse<ConsultarSolicitudResponse>> Consultar(GridRequest<ConsultarSolicitudQuery> grid);        
    }
}
