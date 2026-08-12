using Microsoft.AspNetCore.Mvc;
using Reciclaje.Aplicacion.DTOs.Reciclaje;
using Reciclaje.Aplicacion.Interfaces;

namespace Reciclaje.Web.Controllers
{
    public class ConversionarticuloController : Controller
    {
        private readonly IConversionarticuloServicio _servicio;

        public ConversionarticuloController(IConversionarticuloServicio servicio)
        {
            _servicio = servicio;
        }

        // GET: /Conversionarticulofv
        public async Task<IActionResult> Index()
        {
            var lista = await _servicio.Listar();
            return View(lista);
        }

        // GET: /Conversionarticulo/Crear
        public IActionResult Crear() => View();

        // POST: /Conversionarticulo/Crear
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(ConversionarticuloCrearDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var usuario = User.Identity?.Name ?? "sistema";
            await _servicio.Crear(dto, usuario);
            TempData["Exito"] = "Conversión creada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Conversionarticulo/Editar/5
        public async Task<IActionResult> Editar(int id)
        {
            var dto = await _servicio.ObtenerPorId(id);
            var editar = new ConversionarticuloEditarDto
            {
                ConversionId = dto.ConversionId,
                ArticuloNoInventariado = dto.ArticuloNoInventariado,
                ArticuloReciclaje = dto.ArticuloReciclaje,
                DescripcionArticuloReciclaje = dto.DescripcionArticuloReciclaje
            };
            return View(editar);
        }

        // POST: /Conversionarticulo/Editar/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(ConversionarticuloEditarDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var usuario = User.Identity?.Name ?? "sistema";
            await _servicio.Editar(dto, usuario);
            TempData["Exito"] = "Conversión actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Conversionarticulo/Eliminar/5
        public async Task<IActionResult> Eliminar(int id)
        {
            var dto = await _servicio.ObtenerPorId(id);
            return View(dto);
        }

        // POST: /Conversionarticulo/EliminarConfirmado
        [HttpPost, ActionName("EliminarConfirmado"), ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            await _servicio.Eliminar(id);
            TempData["Exito"] = "Conversión eliminada correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}