using Microsoft.AspNetCore.Mvc;
using Resguardo.Application.Commands.AprobarAmplia;
using Resguardo.Application.Common;
using Resguardo.Web.Authorization;
using Resguardo.Web.Models;

namespace Resguardo.Web.Controllers
{
    [Permission("SERV.APROBAMP")]
    public class ServicioAprobarController : Controller
    {
        private readonly AprobarAmpliaHandler _aprobarAmplia;
        public ServicioAprobarController(AprobarAmpliaHandler aprobarAmplia)
        {
            _aprobarAmplia = aprobarAmplia;
        }        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AprobarAmpliacion([FromBody] AprobarAmpliaCommand formulario)
        {
            formulario.EstAmplia = Constantes.AMPL_APROBADO;
            var aprobar = await _aprobarAmplia.Ejecutar(formulario);
            return Ok(ApiResponse<bool>.Ok(aprobar));
        }        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RechazarAmpliacion([FromBody] AprobarAmpliaCommand formulario)
        {
            formulario.EstAmplia = Constantes.AMPL_RECHAZADO;
            var aprobar = await _aprobarAmplia.Ejecutar(formulario);
            return Ok(ApiResponse<bool>.Ok(aprobar));
        }
    }
}
