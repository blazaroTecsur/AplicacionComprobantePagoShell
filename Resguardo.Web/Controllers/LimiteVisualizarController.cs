using Microsoft.AspNetCore.Mvc;
using Resguardo.Application.Queries.ListarDepartamento;
using Resguardo.Application.Queries.ListarLimites;
using Resguardo.Web.Models;
using Seguridad.Infrastructure.Handler.Authorization;

namespace Resguardo.Web.Controllers
{
    [Permission("LIMT.VER")]
    public class LimiteVisualizarController : Controller
    {
        private readonly ListarLimitesHandler _listar;
        private readonly ListarDptoHandler _dptos;
        public LimiteVisualizarController(
            ListarLimitesHandler listar,
            ListarDptoHandler dptos)
        {
            _listar = listar;
            _dptos = dptos;
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
            var dptos = await _dptos.Ejecutar();
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