using Microsoft.AspNetCore.Mvc;
using Resguardo.Application.Queries.ListarConfig;
using Resguardo.Application.Queries.ListarDepartamento;
using Resguardo.Application.Services;
using Resguardo.Web.Authorization;
using Resguardo.Web.Models;

namespace Resguardo.Web.Controllers
{
    [Permission("CONF.VER")]
    public class ConfigVisualizarController : Controller
    {
        private readonly IMaestroService _maestro;
        private readonly ListarConfigHandler _listarConfig;
        
        public ConfigVisualizarController(
            IMaestroService maestro,
            ListarConfigHandler listarConfig)
        {
            _maestro = maestro;
            _listarConfig = listarConfig;            
        }
        public IActionResult Consulta()
        {
            return View();
        }
        public IActionResult Copiar()
        {
            return PartialView("_Copiar");
        }
        [HttpGet]
        public async Task<IActionResult> ListarDepartamento()
        {
            var dptos = await _maestro.ListarDepartamento();
            if (dptos is not null)
                dptos.ToList().Add(new ListarDptoResponse { Codigo = "", Nombre = "== SELECCIONAR ==" });
            return Ok(dptos);
        }
        [HttpGet]
        public async Task<IActionResult> ListarConfiguracion(DateOnly fecha, string dpto)
        {
            var configs = await _listarConfig.Ejecutar(new ListarConfigQuery() { Fecha = fecha, CodDpto = dpto });
            return Ok(ApiResponse<IEnumerable<ListarConfigResponse>>.Ok(configs));
        }        
    }
}