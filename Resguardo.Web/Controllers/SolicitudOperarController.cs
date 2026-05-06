using Microsoft.AspNetCore.Mvc;
using Resguardo.Application.Commands.ConfirmarServicio;
using Resguardo.Application.Commands.EditarSolicitud;
using Seguridad.Infrastructure.Handler.Authorization;
using Resguardo.Web.Models;

namespace Resguardo.Web.Controllers
{
    public class SolicitudOperarController : Controller
    {
        private readonly ConfirmarServicioHandler _confirmarServicio;
        private readonly EditarSolicitudHandler _editarSolicitud;
        public SolicitudOperarController(
            ConfirmarServicioHandler confirmarServicio,
            EditarSolicitudHandler editarSolicitud)
        {
            _confirmarServicio = confirmarServicio;
            _editarSolicitud = editarSolicitud;
        }

        #region Metodos
        [Permission("SOLC.CONFIRMAR")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarServicios([FromBody] ConfirmarServicioCommand formulario)
        {
            var confirmado = await _confirmarServicio.Ejecutar(formulario);
            return Ok(ApiResponse<bool>.Ok(confirmado));
        }
        [Permission("SOLC.EDITAR")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarSolicitud([FromBody] EditarSolicitudCommand formulario)
        {
            var editado = await _editarSolicitud.Ejecutar(formulario);
            return Ok(ApiResponse<bool>.Ok(editado));
        }
        #endregion
    }
}