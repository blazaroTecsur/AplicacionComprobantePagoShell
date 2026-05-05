using Microsoft.AspNetCore.Mvc;
using Resguardo.Application.Commands.AprobarSolicitud;
using Resguardo.Application.Common;
using Resguardo.Application.Queries.ObtenerConfig;
using Resguardo.Web.Authorization;
using Resguardo.Web.Models;

namespace Resguardo.Web.Controllers
{
    [Permission("SOLC.APROBAR")]
    public class SolicitudAprobarController : Controller
    {
        private readonly AprobarSolicitudHandler _aprobarSolicitud;
        private readonly ObtenerLimitesHandler _obtenerLimite;
        public SolicitudAprobarController(
            AprobarSolicitudHandler aprobarSolicitud,
            ObtenerLimitesHandler obtenerLimite)
        {
            _aprobarSolicitud = aprobarSolicitud;
            _obtenerLimite = obtenerLimite;
        }
        #region Metodos
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AprobarSolicitud([FromBody] AprobarSolicitudCommand formulario)
        {
            formulario.CodEstado = Constantes.COD_ESTADO_APROBJEFE;
            var aprobado = await _aprobarSolicitud.Ejecutar(formulario);
            return Ok(ApiResponse<bool>.Ok(aprobado));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AnularSolicitud([FromBody] AprobarSolicitudCommand formulario)
        {
            formulario.CodEstado = Constantes.COD_ESTADO_ANULADO;
            var aprobado = await _aprobarSolicitud.Ejecutar(formulario);
            return Ok(ApiResponse<bool>.Ok(aprobado));
        }
        [HttpGet]
        public async Task<IActionResult> ObtenerLimite(DateOnly fecha, string dpto)
        {
            var limites = await _obtenerLimite.Ejecutar(new ObtenerLimitesQuery() { Fecha = fecha, CodDpto = dpto });
            return Ok(ApiResponse<IEnumerable<ObtenerLimitesResponse>>.Ok(limites));
        }
        #endregion
    }
}