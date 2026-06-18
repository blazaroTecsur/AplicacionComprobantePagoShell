using Microsoft.AspNetCore.Mvc;
using Reciclaje.Aplicacion.DTOs.Reciclaje;
using Reciclaje.Aplicacion.Interfaces;

namespace Reciclaje.Web.Controllers
{
    /// <summary>
    /// Controlador responsable de la vista ConfirmarVale.
    /// Gestiona: búsqueda, confirmación, rechazo y reporte Excel de vales.
    /// </summary>
    public class ConfirmarValeController : Controller
    {
        private readonly IValeConfirmacionServicio _confirmacionServicio;
        private readonly IValeConfirmacionReporteServicio _reporteServicio;

        public ConfirmarValeController(
            IValeConfirmacionServicio confirmacionServicio,
            IValeConfirmacionReporteServicio reporteServicio)
        {
            _confirmacionServicio = confirmacionServicio;
            _reporteServicio = reporteServicio;
        }

        // ── GET: /ConfirmarVale ──────────────────────────────────────
        public IActionResult Index() =>
            View(new ValeConfirmacionListaViewModel());

        // ── POST: /ConfirmarVale/BuscarValesConfirmacion ─────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> BuscarValesConfirmacion(ValeRecuperoBuscarDto filtros)
        {
            var vm = await _confirmacionServicio.BuscarValesConfirmacion(filtros);
            return View("Index", vm);
        }

        // ── POST: /ConfirmarVale/GuardarConfirmacion ─────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarConfirmacion(
            string accion,
            ValeRecuperoBuscarDto filtros,
            List<ValeConfirmacionGuardarDto> confirmaciones)
        {
            var usuario = User.Identity?.Name ?? "sistema";

            if (accion == "Rechazar")
            {
                var seleccionados = confirmaciones.Where(c => c.Seleccionado).ToList();
                if (!seleccionados.Any())
                {
                    TempData["Aviso"] = "Debe seleccionar al menos un vale para rechazar.";
                    return RedirectToAction(nameof(Index));
                }
                await _confirmacionServicio.RechazarVales(confirmaciones, usuario);
                TempData["Exito"] = "Los vales seleccionados fueron rechazados y regresaron a estado Pendiente.";
            }
            else
            {
                await _confirmacionServicio.GuardarConfirmacion(confirmaciones, usuario);
                TempData["Exito"] = "Confirmación guardada correctamente.";
            }

            return RedirectToAction(nameof(Index));
        }

        // ── GET: /ConfirmarVale/GenerarReporteExcel ──────────────────
        /// <summary>
        /// Genera y descarga el reporte Excel "Vale de Recupero de la SST — Confirmación"
        /// agrupado por SRO, usando los filtros activos de la búsqueda.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GenerarReporteExcel(
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

            var excelBytes = await _reporteServicio.GenerarExcel(filtros);

            var nombreArchivo = $"ValeRecupero_Confirmacion_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
            return File(
                excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                nombreArchivo);
        }
    }
}
