using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shell.Web.Services;
using System.Reflection;

namespace Shell.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly ApiService _apiService;
        public HomeController(IWebHostEnvironment env, ApiService apiService)
        {
            _env = env;
            _apiService = apiService;
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
    }
}