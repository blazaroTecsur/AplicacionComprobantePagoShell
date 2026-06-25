using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shell.Web.Models.ActualizarDatos;
using Shell.Web.Services;
using System.Reflection;

namespace Shell.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly ApiService _apiService;
        private readonly ILogger<HomeController> _logger;
        public HomeController(IWebHostEnvironment env, ApiService apiService, ILogger<HomeController> logger)
        {
            _env = env;
            _apiService = apiService;
            _logger = logger;
        }
        public async Task<IActionResult> Index()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version?.ToString();

            var usuario = await _apiService.ObtenerUsuario();
            ViewBag.Tenant = usuario.NomTenant;
            ViewBag.Entorno = _env.EnvironmentName;
            ViewBag.Version = version;
            return View(usuario);
        }
        public async Task<IActionResult> ActualizarDatos([FromBody] ActualizarViewModel usuario)
        {
            try
            {
                var result = await _apiService.ActualizarDatos(usuario);
                if (!result.Success)
                    return BadRequest(result.Error);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                return StatusCode(500,
                    new ApiErrorDetail
                    {
                        Code = "INTERNAL_ERROR",
                        UserMessage =
                            "Ocurrió un error inesperado. Inténtelo nuevamente."
                    });
            }
        }
    }
}