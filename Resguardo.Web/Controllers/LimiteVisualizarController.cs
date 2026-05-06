using Microsoft.AspNetCore.Mvc;
using Resguardo.Application.Queries.ListarDepartamento;
using Resguardo.Application.Queries.ListarLimites;
using Resguardo.Application.Services;
using Seguridad.Infrastructure.Handler.Authorization;
using Resguardo.Web.Models;

namespace Resguardo.Web.Controllers
{
    [Permission("LIMT.VER")]
    public class LimiteVisualizarController : Controller
    {
        private readonly IMaestroService _maestro;
        private readonly ListarLimitesHandler _listar;
        
        public LimiteVisualizarController(
            IMaestroService maestro,
            ListarLimitesHandler listar)
        {
            _maestro = maestro;
            _listar = listar;            
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
            var configs = await _listar.Ejecutar(new ListarLimitesQuery() { Fecha = fecha, CodDpto = dpto });
            return Ok(ApiResponse<IEnumerable<ListarLimitesResponse>>.Ok(configs));
        }        
    }
}