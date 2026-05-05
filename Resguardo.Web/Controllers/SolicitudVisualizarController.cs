using Microsoft.AspNetCore.Mvc;
using Resguardo.Application.Common;
using Resguardo.Application.Queries.ConsultarCapataz;
using Resguardo.Application.Queries.ConsultarSolicitud;
using Resguardo.Application.Queries.ListarGenerico;
using Resguardo.Application.Queries.ListarServicio;
using Resguardo.Application.Queries.ListarServicioProv;
using Resguardo.Application.Queries.ObtenerOrden;
using Resguardo.Application.Queries.ObtenerSolicitud;
using Resguardo.Application.Services;
using Resguardo.Web.Authorization;
using Resguardo.Web.Models;

namespace Resguardo.Web.Controllers
{
    [Permission("SOLC.VER")]    
    public class SolicitudVisualizarController : Controller
    {        
        private readonly ObtenerSolicitudHandler _obtenerSolicitud;
        private readonly ConsultarSolicitudHandler _consultarSolicitud;
        private readonly ListarGenericoHandler _listarGenerico;
        private readonly ListarServicioHandler _listarServicio;
        private readonly ListarServicioProvHandler _listarServicioCtta;
        private readonly IInforService _orden;
        private readonly IMaestroService _maestro;
        public SolicitudVisualizarController(         
            ObtenerSolicitudHandler obtenerSolicitud,
            ConsultarSolicitudHandler consultarSolicitud,
            ListarGenericoHandler listarGenerico,
            ListarServicioHandler listarServicio,
            ListarServicioProvHandler listarServicioCtta,
            IInforService orden,
            IMaestroService maestro)
        {           
            _obtenerSolicitud = obtenerSolicitud;
            _consultarSolicitud = consultarSolicitud;
            _listarGenerico = listarGenerico;
            _listarServicio = listarServicio;
            _listarServicioCtta = listarServicioCtta;
            _orden = orden;
            _maestro = maestro;
        }

        #region Vistas        
        public IActionResult Consulta()
        {
            return View();
        }
        public IActionResult Formulario()
        {
            return PartialView("_Formulario");
        }
        public IActionResult Servicio()
        {
            return PartialView("_Servicio");
        }
        public IActionResult Capataz()
        {
            return PartialView("_Capataz");
        }
        public IActionResult Mapa()
        {
            return PartialView("_Mapa");
        }        
        #endregion

        #region Metodos
        [HttpGet]
        public async Task<IActionResult> ObtenerSolicitud(int id)
        {
            var solicitud = await _obtenerSolicitud.Ejecutar(id);
            return Ok(ApiResponse<ObtenerSolicitudResponse>.Ok(solicitud));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConsultarSolicitudes([FromBody] GridRequest<ConsultarSolicitudQuery> grid)
        {
            var solicitudes = await _consultarSolicitud.Ejecutar(grid);
            return Ok(ApiResponse<GridResponse<ConsultarSolicitudResponse>>.Ok(solicitudes));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConsultarCapataces([FromBody] GridRequest<ConsultarCapatazQuery> grid)
        {
            var capataces = await _maestro.ConsultarCapataz(grid);
            return Ok(ApiResponse<GridResponse<ConsultarCapatazResponse>>.Ok(capataces));
        }
        [HttpGet]
        public async Task<IActionResult> ListarServicios(int id)
        {
            var servicios = await _listarServicio.Ejecutar(id);
            return Ok(ApiResponse<IEnumerable<ListarServicioResponse>>.Ok(servicios));
        }
        [HttpGet]
        public async Task<IActionResult> BuscarOrden(string nroSro)
        {
            var orden = await _orden.ObtenerOrden(nroSro);
            return Ok(ApiResponse<ObtenerOrdenResponse>.Ok(orden));
        }
        [HttpGet]
        public async Task<IActionResult> ListarGenerico()
        {
            var genericos = await _listarGenerico.Ejecutar([
                Constantes.TIP_TIPO,
                Constantes.TIP_FLUJO,
                Constantes.TIP_ESTADO,
                Constantes.TIP_SERVICIO]);
            if (genericos is not null)
                genericos.ToList().Add(new ListarGenericoResponse { Id = 0, Descripcion = "== SELECCIONAR ==" });
            return Ok(genericos);
        }
        #endregion
    }
}