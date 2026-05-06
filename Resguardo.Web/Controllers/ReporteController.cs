using Microsoft.AspNetCore.Mvc;
using Resguardo.Application.Queries.ReporteEfectivo;
using Resguardo.Application.Queries.ReporteSolicitud;
using Seguridad.Infrastructure.Handler.Authorization;

namespace Resguardo.Web.Controllers
{
    public class ReporteController : Controller
    {
        private readonly ReporteSolicitudHandler _rptSolicitud;
        private readonly ReporteEfectivoHandler _rptEfectivo;
        public ReporteController(ReporteSolicitudHandler rptSolicitud, ReporteEfectivoHandler rptEfectivo)
        {
            _rptSolicitud = rptSolicitud;
            _rptEfectivo = rptEfectivo;
        }
        public IActionResult Filtros()
        {
            return PartialView("_Filtros");
        }
        [Permission("SOLC.RPT")]
        [HttpGet]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReporteSolicitud(DateOnly fechaDsd, DateOnly fechaHst, int? flujo)
        {
            var fileBytes = await _rptSolicitud.Ejecutar(new ReporteSolicitudQuery() { 
                FechaDsd = fechaDsd, FechaHst = fechaHst, IdFlujo = flujo });            
            var fileName = $"rpt_solicitud_{Guid.NewGuid()}.xlsx";
            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName
            );
        }
        [Permission("SERV.RPT")]
        [HttpGet]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReporteEfectivo(DateOnly fechaDsd, DateOnly fechaHst, int? servicio)
        {
            var fileBytes = await _rptEfectivo.Ejecutar(
                new ReporteEfectivoQuery() { FechaDsd = fechaDsd, FechaHst = fechaHst, IdServicio = servicio });
            var fileName = $"rpt_servicio_{Guid.NewGuid()}.xlsx";
            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName
            );
        }
    }
}