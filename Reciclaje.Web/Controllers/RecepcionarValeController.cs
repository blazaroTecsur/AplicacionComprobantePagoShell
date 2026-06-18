using Microsoft.AspNetCore.Mvc;
using Reciclaje.Aplicacion.DTOs.Reciclaje;
using Reciclaje.Aplicacion.Interfaces;

namespace Reciclaje.Web.Controllers
{
    /// <summary>
    /// Controlador responsable de la vista RecepcionarVale.
    /// Gestiona: búsqueda de vales, registro de recepción y generación del reporte PDF.
    /// </summary>
    public class RecepcionarValeController : Controller
    {
        private readonly IValeRecepcionServicio _recepcionServicio;
        private readonly IValeRecuperoReporteServicio _reporteServicio;

        public RecepcionarValeController(
            IValeRecepcionServicio recepcionServicio,
            IValeRecuperoReporteServicio reporteServicio)
        {
            _recepcionServicio = recepcionServicio;
            _reporteServicio = reporteServicio;
        }

        // ── GET: /RecepcionarVale ────────────────────────────────────
        public IActionResult Index() =>
            View(new ValeRecuperoListaViewModel());

        // ── POST: /RecepcionarVale/BuscarVales ───────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> BuscarVales(ValeRecuperoBuscarDto filtros)
        {
            var vm = await _recepcionServicio.BuscarVales(filtros);
            return View("Index", vm);
        }

        // ── POST: /RecepcionarVale/GuardarRecepcion ──────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarRecepcion(
            ValeRecuperoBuscarDto filtros,
            List<ValeRecepcionDto> recepciones)
        {
            var usuario = User.Identity?.Name ?? "sistema";
            await _recepcionServicio.GuardarRecepcion(recepciones, usuario);
            TempData["Exito"] = "Recepción guardada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // ── GET: /RecepcionarVale/GenerarReporte ─────────────────────
        /// <summary>
        /// Genera y descarga el reporte PDF "Vale de Recupero de la SST"
        /// usando los mismos filtros de la búsqueda actual.
        /// Los filtros se reciben como query-string para poder llamarlo
        /// desde un enlace directo sin necesidad de un segundo POST.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GenerarReporte(
            string? NumeroSro,
            string? NumeroVale,
            string? CodigoArticuloReciclaje,
            string? DescripcionArticuloReciclaje,
            DateTime? FechaVale)
        {
            var filtros = new ValeRecuperoBuscarDto
            {
                NumeroSro = NumeroSro,
                NumeroVale = NumeroVale,
                CodigoArticuloReciclaje = CodigoArticuloReciclaje,
                DescripcionArticuloReciclaje = DescripcionArticuloReciclaje,
                FechaVale = FechaVale
            };

            var pdfBytes = await _reporteServicio.GenerarPdf(filtros);

            var nombreArchivo = $"ValeRecupero_SST_{DateTime.Now:yyyyMMdd_HHmm}.pdf";
            return File(pdfBytes, "application/pdf", nombreArchivo);
        }
    }
}
