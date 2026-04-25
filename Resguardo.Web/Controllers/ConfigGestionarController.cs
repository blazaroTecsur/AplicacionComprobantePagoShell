using Microsoft.AspNetCore.Mvc;
using Resguardo.Application.Commands.CopiarConfig;
using Resguardo.Application.Commands.RegistrarConfig;
using Resguardo.Web.Authorization;
using Resguardo.Web.Models;

namespace Resguardo.Web.Controllers
{
    public class ConfigGestionarController : Controller
    {
        private readonly RegistrarConfigHandler _registrarConfig;
        private readonly CopiarConfigHandler _copiarConfig;
        public ConfigGestionarController(            
            RegistrarConfigHandler registrarConfig,
            CopiarConfigHandler copiarConfig)
        {            
            _registrarConfig = registrarConfig;
            _copiarConfig = copiarConfig;
        }
        [Permission("CONF.CREAR")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarConfig([FromBody] RegistrarConfigCommand formulario)
        {
            var registro = await _registrarConfig.Ejecutar(formulario);
            return Ok(ApiResponse<bool>.Ok(registro));
        }
        [Permission("CONF.COPIAR")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CopiarConfig([FromBody] CopiarConfigCommand formulario)
        {
            var copiar = await _copiarConfig.Ejecutar(formulario);
            return Ok(ApiResponse<bool>.Ok(copiar));
        }
    }
}
