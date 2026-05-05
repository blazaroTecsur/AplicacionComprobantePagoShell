using Resguardo.Application.Queries.ReporteEfectivo;
using Resguardo.Application.Queries.ReporteSolicitud;

namespace Resguardo.Application.Interfaces
{
    public interface IReporteQueryService
    {
        Task<IEnumerable<ReporteSolicitudResponse>> ReporteSolicitudes(ReporteSolicitudQuery filtro);
        Task<IEnumerable<ReporteEfectivoResponse>> ReporteEfectivos(ReporteEfectivoQuery filtro);
    }
}