using Seguridad.Infrastructure.Handler.Authorization;
using ComprobantePago.Application.DTOs.Comprobante.Requests;
using ComprobantePago.Application.Common;
using ComprobantePago.Application.Interfaces.QueryServices;
using ComprobantePago.Application.Interfaces.Repositories;
using ComprobantePago.Application.Interfaces.Services.Maestros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComprobantePago.Web.Controllers
{
    //[Authorize]
    [Permission("COMP.VER")]
    [Route("Comprobante")]
    public class ComprobanteConsultarController : Controller
    {
        private readonly IComprobanteQueryService _queryService;
        private readonly IMaestrosQueryService    _maestrosService;
        private readonly IProveedorService        _proveedorService;
        private readonly IComprobanteRepository   _repository;
        private readonly ILogger<ComprobanteConsultarController> _logger;

        public ComprobanteConsultarController(
            IComprobanteQueryService queryService,
            IMaestrosQueryService   maestrosService,
            IProveedorService       proveedorService,
            IComprobanteRepository  repository,
            ILogger<ComprobanteConsultarController> logger)
        {
            _queryService     = queryService;
            _maestrosService  = maestrosService;
            _proveedorService = proveedorService;
            _repository       = repository;
            _logger           = logger;
        }

        // ── Vistas MVC ──────────────────────────────────────────

        [HttpGet("Index")]
        public IActionResult Index() => View();

        [HttpGet("Detalle")]
        public IActionResult Detalle() => View();

        // ── Combos ──────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> ObtenerTiposDocumento()
            => Ok(await _maestrosService.ObtenerTiposDocumentoAsync());

        [HttpGet]
        public async Task<IActionResult> ObtenerTiposSunat()
            => Ok(await _maestrosService.ObtenerTiposSunatAsync());

        [HttpGet]
        public async Task<IActionResult> ObtenerMonedas()
            => Ok(await _maestrosService.ObtenerMonedasAsync());

        [HttpGet]
        public async Task<IActionResult> ObtenerLugaresPago()
            => Ok(await _maestrosService.ObtenerLugaresPagoAsync());

        [HttpGet]
        public async Task<IActionResult> ObtenerTiposDetraccion()
            => Ok(await _maestrosService.ObtenerTiposDetraccionAsync());

        [HttpGet]
        public async Task<IActionResult> ObtenerTipos()
            => Ok(await _maestrosService.ObtenerTiposDocumentoAsync());

        [HttpGet]
        public async Task<IActionResult> ObtenerEstados()
            => Ok(await _maestrosService.ObtenerEstadosAsync());

        [HttpGet]
        public async Task<IActionResult> ObtenerEmpleados([FromQuery] string filtro = "")
            => Ok(await _maestrosService.ObtenerEmpleadosAsync(filtro));

        [HttpGet]
        public async Task<IActionResult> ObtenerProveedores([FromQuery] string filtro = "")
            => Ok(await _proveedorService.ObtenerProveedoresAsync(filtro));

        // ── Consultas comprobante ────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> Buscar([FromBody] BuscarComprobanteDto filtros)
            => Ok(await _queryService.BuscarAsync(filtros));

        [HttpGet]
        public async Task<IActionResult> ObtenerDetalle([FromQuery] string folio)
        {
            var data = await _queryService.ObtenerDetalleAsync(folio);
            return data is null
                ? Ok(BaseResponse.Error("Comprobante no encontrado."))
                : Ok(data);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerPdf([FromQuery] string folio, [FromQuery] bool descargar = false)
        {
            var pdf = await _queryService.ObtenerPdfAsync(folio);
            if (pdf is null) return NotFound();
            var nombre = $"Comprobante_{folio}.pdf";
            return descargar
                ? File(pdf, "application/pdf", nombre)
                : File(pdf, "application/pdf");
        }

        [HttpGet]
        public async Task<IActionResult> DocumentosElectronicos([FromQuery] string folio)
            => Ok(await _queryService.ObtenerDocumentosElectronicosAsync(folio));

        // ── Imputaciones / catálogos lectura ─────────────────────

        [HttpGet]
        public async Task<IActionResult> ObtenerImputaciones([FromQuery] string folio)
            => Ok(await _queryService.ObtenerImputacionesAsync(folio));

        [HttpGet]
        public async Task<IActionResult> ObtenerCuentasContables([FromQuery] string filtro = "")
            => Ok(await _maestrosService.ObtenerCuentasContablesAsync(filtro));

        [HttpGet]
        public async Task<IActionResult> ObtenerCodigosUnidad(
            string campo, int unidad, string codigo, string filtro = "")
            => Ok(await _maestrosService.ObtenerCodigosUnidadAsync(campo, unidad, codigo, filtro));

        [HttpGet]
        public async Task<IActionResult> DescargarPlantillaImputacion()
        {
            var archivo = await _queryService.ObtenerPlantillaImputacionAsync();
            return File(archivo,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "PlantillaImputacion.xlsx");
        }

        [HttpGet]
        public async Task<IActionResult> DescargarDocumento([FromQuery] int id)
        {
            var doc = await _repository.DescargarDocumentoAsync(id);
            if (doc is null) return NotFound();
            var ext = Path.GetExtension(doc.NombreArchivo).TrimStart('.').ToLowerInvariant();
            var contentType = ext == "pdf" ? "application/pdf" : "application/octet-stream";
            return File(doc.Contenido, contentType, doc.NombreArchivo);
        }
    }
}
