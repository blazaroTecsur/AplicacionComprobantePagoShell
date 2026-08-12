using Microsoft.AspNetCore.Mvc;
using Reciclaje.Aplicacion.DTOs.Reciclaje;
using Reciclaje.Aplicacion.Interfaces;

namespace Reciclaje.Web.Controllers
{
    public class TareaOrdenCompraController : Controller
    {
        private readonly ITareaOrdenCompraServicio _servicio;

        public TareaOrdenCompraController(ITareaOrdenCompraServicio servicio)
        {
            _servicio = servicio;
        }

        // GET: /TareaOrdenCompra
        public async Task<IActionResult> Index()
        {
            var lista = await _servicio.ObtenerTodos();
            return View(lista);
        }

        // GET: /TareaOrdenCompra/Crear
        public IActionResult Crear()
        {
            return View(new TareaOrdenCompraCrearDto
            {
                Anno = (short)DateTime.Now.Year,
                Mes = (byte)DateTime.Now.Month,
                Estado = "Pendiente"
            });
        }

        // POST: /TareaOrdenCompra/Crear
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(TareaOrdenCompraCrearDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            try
            {
                var usuario = User.Identity?.Name ?? "sistema";
                await _servicio.Crear(dto, usuario);
                TempData["Exito"] = "Orden de Compra creada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }

        // GET: /TareaOrdenCompra/Editar/5
        public async Task<IActionResult> Editar(int id)
        {
            try
            {
                var dto = await _servicio.ObtenerParaEditar(id);
                return View(dto);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /TareaOrdenCompra/Editar
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(TareaOrdenCompraEditarDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            try
            {
                var usuario = User.Identity?.Name ?? "sistema";
                await _servicio.Editar(dto, usuario);
                TempData["Exito"] = "Orden de Compra actualizada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }

        // GET: /TareaOrdenCompra/Eliminar/5
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                var dto = await _servicio.ObtenerParaEditar(id);
                return View(dto);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /TareaOrdenCompra/EliminarConfirmado
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            try
            {
                await _servicio.Eliminar(id);
                TempData["Exito"] = "Orden de Compra eliminada correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}