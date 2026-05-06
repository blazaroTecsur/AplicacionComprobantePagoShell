using Microsoft.AspNetCore.Mvc;
using Resguardo.Application.Commands.AsignarEfectivo;
using Resguardo.Application.Commands.CerrarServicio;
using Resguardo.Application.Common;
using Resguardo.Application.Queries.ConsultarServicio;
using Resguardo.Application.Queries.ConsultarSolicitud;
using Resguardo.Application.Queries.ListarEfectivos;
using Resguardo.Application.Queries.ListarGenerico;
using Resguardo.Application.Queries.ObtenerPersonal;
using Resguardo.Application.Queries.ObtenerSolicitudFolio;
using Resguardo.Web.Models;
using Seguridad.Infrastructure.Handler.Authorization;

namespace Resguardo.Web.Controllers
{
    [Permission("SERV.VER")]
    public class ServicioVisualizarController : Controller
    {
        private readonly ListarGenericoHandler _listarGenerico;
        private readonly ConsultarServicioHandler _consultarServicio;
        private readonly AsignarEfectivoHandler _asignarEfectivo;
        private readonly ListarEfectivoHandler _listarEfectivo;
        private readonly CerrarServicioHandler _cerrarEfectivo;
        private readonly ObtenerPersonalHandler _obtenerPersonal;
        private readonly ObtenerSolicitudFolioHandler _obtenerSolicitud;
        public ServicioVisualizarController(
            ListarGenericoHandler listarGenerico,
            ConsultarServicioHandler consultarServicio,
            AsignarEfectivoHandler asignarEfectivo,
            ListarEfectivoHandler listarEfectivo,
            CerrarServicioHandler cerrarEfectivo,
            ObtenerPersonalHandler obtenerPersonal,
            ObtenerSolicitudFolioHandler obtenerSolicitud)
        {
            _listarGenerico = listarGenerico;
            _consultarServicio = consultarServicio;
            _asignarEfectivo = asignarEfectivo;
            _listarEfectivo = listarEfectivo;
            _cerrarEfectivo = cerrarEfectivo;
            _obtenerPersonal = obtenerPersonal;
            _obtenerSolicitud = obtenerSolicitud;
        }
        public IActionResult Consulta()
        {
            return View();
        }
        public IActionResult Efectivo()
        {
            return PartialView("_Efectivo");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConsultarServicios([FromBody] GridRequest<ConsultarServicioQuery> grid)
        {
            var solicitudes = await _consultarServicio.Ejecutar(grid);
            return Ok(ApiResponse<GridResponse<ConsultarServicioResponse>>.Ok(solicitudes));
        }
        [HttpGet]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ObtenerSolicitud(string folio)
        {
            var solicitud = await _obtenerSolicitud.Ejecutar(folio);
            return Ok(ApiResponse<ObtenerSolicitudFolioResponse>.Ok(solicitud));
        }
        [HttpGet]
        public async Task<IActionResult> ListarGenerico()
        {
            var genericos = await _listarGenerico.Ejecutar([Constantes.TIP_PROVEEDOR, Constantes.TIP_SERVICIO]);
            if (genericos is not null)
                genericos.ToList().Add(new ListarGenericoResponse { Id = 0, Descripcion = "== SELECCIONAR ==" });
            return Ok(genericos);
        }        
        [HttpGet]
        public async Task<IActionResult> ListarEfectivos(int id)
        {
            var efectivos = await _listarEfectivo.Ejecutar(id);
            return Ok(ApiResponse<IEnumerable<ListarEfectivoResponse>>.Ok(efectivos));
        }        
        [HttpGet]
        public async Task<IActionResult> ObtenerPersonal(string dni)
        {
            var personal = await _obtenerPersonal.Ejecutar(dni);
            return Ok(ApiResponse<ObtenerPersonalResponse>.Ok(personal));
        }
    }
}