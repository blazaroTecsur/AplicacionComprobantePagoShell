using Microsoft.AspNetCore.Mvc;
using Reciclaje.Aplicacion.DTOs.Reciclaje;
using Reciclaje.Aplicacion.Interfaces;
using Reciclaje.Dominio.Interfaces;

namespace Reciclaje.Web.Controllers
{
    /// <summary>
    /// Controlador responsable de la vista Index de Vale de Recupero.
    /// Gestiona: búsqueda de líneas SRO, creación de SRO y SROLinea,
    /// y generación de Vale Específico / Consolidado.
    /// </summary>
    public class ValeRecuperoController : Controller
    {
        private readonly IValeRecuperoGeneracionServicio _generacionServicio;
        private readonly ITareaOrdenCompraRepositorio _tareaRepo;

        public ValeRecuperoController(
            IValeRecuperoGeneracionServicio generacionServicio,
            ITareaOrdenCompraRepositorio tareaRepo)
        {
            _generacionServicio = generacionServicio;
            _tareaRepo = tareaRepo;
        }

        // ── GET: /ValeRecupero ───────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            await SetPeriodoActualAsync();
            return View(new ValeRecuperoViewModel());
        }

        // ── POST: /ValeRecupero/Buscar ───────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Buscar(ValeRecuperoBusquedaDto filtros)
        {
            await SetPeriodoActualAsync();
            var vm = await _generacionServicio.Buscar(filtros);
            return View("Index", vm);
        }

        // ── POST: /ValeRecupero/Generar ──────────────────────────────
        /// <summary>
        /// Genera el tipo de vale seleccionado (Específico o Consolidado)
        /// para las líneas marcadas en la grilla.
        /// </summary>
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Generar(ValeRecuperoBusquedaDto filtros)
        {
            if (!filtros.LineasSeleccionadas.Any())
            {
                TempData["Error"] = "Debe seleccionar al menos una línea.";
                var vm = await _generacionServicio.Buscar(filtros);
                await SetPeriodoActualAsync();
                return View("Index", vm);
            }

            var usuario = User.Identity?.Name ?? "sistema";

            // Validar que ninguna línea seleccionada esté en estado Procesado
            var lineasProcesadas = await _generacionServicio
                .ObtenerLineasProcesadas(filtros.LineasSeleccionadas);

            if (lineasProcesadas.Any())
            {
                TempData["Error"] = $"No se puede generar el vale. Las siguientes líneas ya fueron procesadas: " +
                                    string.Join(", ", lineasProcesadas);
                var vmError = await _generacionServicio.Buscar(filtros);
                await SetPeriodoActualAsync();
                return View("Index", vmError);
            }

            if (filtros.GenerarEspecifico)
            {
                await _generacionServicio.GenerarValeEspecifico(filtros.LineasSeleccionadas, usuario);
            }
            else if (filtros.GenerarConsolidado)
            {
                await _generacionServicio.GenerarValeConsolidado(filtros.LineasSeleccionadas, usuario);
            }
            else
            {
                TempData["Error"] = "Seleccione un tipo de vale: Específico o Consolidado.";
                var vm = await _generacionServicio.Buscar(filtros);
                await SetPeriodoActualAsync();
                return View("Index", vm);
            }

            TempData["Exito"] = "Vale de recupero generado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // ── Helper: verifica si existe TareaOrdenCompra del período actual ──
        private async Task SetPeriodoActualAsync()
        {
            var existe = await _tareaRepo.ExistePeriodoActual();
            ViewBag.TienePeriodoActual = existe;

            if (!existe)
            {
                var ahora = DateTime.Now;
                ViewBag.PeriodoActualMsg =
                    $"No existe una Orden de Compra configurada para el período actual " +
                    $"({ahora:MMMM yyyy}). Registre la tarea antes de buscar o generar vales.";
            }
        }
    }
}
