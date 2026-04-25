using Microsoft.AspNetCore.Mvc;
using Resguardo.Application.Commands.AmpliarServicio;
using Resguardo.Application.Commands.AprobarAmplia;
using Resguardo.Application.Commands.AsignarEfectivo;
using Resguardo.Application.Commands.CerrarServicio;
using Resguardo.Application.Common;
using Resguardo.Web.Authorization;
using Resguardo.Web.Models;

namespace Resguardo.Web.Controllers
{
    public class ServicioOperarController : Controller
    {
        private readonly AsignarEfectivoHandler _asignarEfectivo;
        private readonly CerrarServicioHandler _cerrarEfectivo;
        private readonly AmpliarServicioHandler _ampliarServicio;
        private readonly AprobarAmpliaHandler _aprobarAmplia;
        public ServicioOperarController(
            AsignarEfectivoHandler asignarEfectivo,
            CerrarServicioHandler cerrarEfectivo,
            AmpliarServicioHandler ampliarServicio,
            AprobarAmpliaHandler aprobarAmplia)
        {
            _asignarEfectivo = asignarEfectivo;
            _cerrarEfectivo = cerrarEfectivo;
            _ampliarServicio = ampliarServicio;
            _aprobarAmplia = aprobarAmplia;
        }
        public IActionResult Index()
        {
            return View();
        }
        [Permission("SERV.CERRAR")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CerrarServicio([FromBody] CerrarServicioCommand formulario)
        {
            var cerrado = await _cerrarEfectivo.Ejecutar(formulario);
            return Ok(ApiResponse<bool>.Ok(cerrado));
        }
        [Permission("SERV.ASIGNAR")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarEfectivos([FromBody] AsignarEfectivoCommand formulario)
        {
            var asignado = await _asignarEfectivo.Ejecutar(formulario);
            return Ok(ApiResponse<bool>.Ok(asignado));
        }
        //[Permission("SERV.ASIGNAR")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AmpliarServicio([FromBody] AmpliarServicioCommand formulario)
        {
            var ampliar = await _ampliarServicio.Ejecutar(formulario);
            return Ok(ApiResponse<bool>.Ok(ampliar));
        }
        //[Permission("SERV.ASIGNAR")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AprobarAmpliacion([FromBody] AprobarAmpliaCommand formulario)
        {
            formulario.EstAmplia = Constantes.AMPL_APROBADO;
            var aprobar = await _aprobarAmplia.Ejecutar(formulario);
            return Ok(ApiResponse<bool>.Ok(aprobar));
        }
        //[Permission("SERV.ASIGNAR")]
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