using Microsoft.AspNetCore.Mvc;
using Resguardo.Application.Commands.ActualizarSolicitud;
using Resguardo.Application.Commands.RegistrarSolicitud;
using Resguardo.Web.Authorization;
using Resguardo.Web.Models;

namespace Resguardo.Web.Controllers
{    
    public class SolicitudGestionarController : Controller
    {
        private readonly RegistrarSolicitudHandler _registrarSolicitud;        
        private readonly ActualizarSolicitudHandler _actualizarSolicitud;
        public SolicitudGestionarController(
            RegistrarSolicitudHandler registrarSolicitud,
            ActualizarSolicitudHandler actualizarSolicitud)
        {
            _registrarSolicitud = registrarSolicitud;            
            _actualizarSolicitud = actualizarSolicitud;
        }

        #region Metodos
        [Permission("SOLC.CREAR")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarSolicitud([FromBody] RegistrarSolicitudCommand formulario)
        {
            var solicitud = await _registrarSolicitud.Ejecutar(formulario);
            return Ok(ApiResponse<RegistrarSolicitudResponse>.Ok(solicitud));
        }
        [Permission("SOLC.MODIF")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarSolicitud([FromBody] ActualizarSolicitudCommand formulario)
        {
            var id = await _actualizarSolicitud.Ejecutar(formulario);
            return Ok(ApiResponse<int>.Ok(id));
        }
        #endregion
    }
}
