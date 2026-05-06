using Microsoft.AspNetCore.Mvc;
using Resguardo.Application.Commands.CopiarConfig;
using Resguardo.Application.Commands.RegistrarConfig;
using Seguridad.Infrastructure.Handler.Authorization;
using Resguardo.Web.Models;

namespace Resguardo.Web.Controllers
{
    public class LimiteGestionarController : Controller
    {
        private readonly RegistrarConfigHandler _registrarConfig;
        private readonly CopiarConfigHandler _copiarConfig;
        public LimiteGestionarController(            
            RegistrarConfigHandler registrarConfig,
            CopiarConfigHandler copiarConfig)
        {            
            _registrarConfig = registrarConfig;
            _copiarConfig = copiarConfig;
        }
        [Permission("LIMT.CREAR")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarConfig([FromBody] RegistrarConfigCommand formulario)
        {
            var registro = await _registrarConfig.Ejecutar(formulario);
            return Ok(ApiResponse<bool>.Ok(registro));
        }
        [Permission("LIMT.COPIAR")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CopiarConfig([FromBody] CopiarConfigCommand formulario)
        {
            var copiar = await _copiarConfig.Ejecutar(formulario);
            return Ok(ApiResponse<bool>.Ok(copiar));
        }
    }
}
