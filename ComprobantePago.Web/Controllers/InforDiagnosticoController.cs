using ComprobantePago.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComprobantePago.Web.Controllers
{
    /// <summary>
    /// Endpoints de diagnóstico para verificar la conectividad con la API IDO de Infor Syteline.
    /// Solo disponible en Development. Devuelve el JSON crudo de la respuesta IDO.
    /// </summary>
    [AllowAnonymous]
    [Route("api/infor")]
    [ApiController]
    public class InforDiagnosticoController : ControllerBase
    {
        private readonly ISytelineIdoService _ido;
        private readonly IWebHostEnvironment _env;

        public InforDiagnosticoController(
            ISytelineIdoService ido,
            IWebHostEnvironment env)
        {
            _ido = ido;
            _env = env;
        }

        /// <summary>
        /// Verifica conectividad: obtiene el token y lista las configuraciones IDO disponibles.
        /// GET /api/infor/ping
        /// </summary>
        [HttpGet("ping")]
        public async Task<IActionResult> Ping(CancellationToken ct)
        {
            if (!_env.IsDevelopment())
                return NotFound();

            var resultado = await _ido.ObtenerConfiguracionesAsync(ct);
            return Ok(resultado);
        }

        /// <summary>
        /// Devuelve el schema (propiedades) de un IDO.
        /// GET /api/infor/idoinfo/{nombre}
        /// </summary>
        [HttpGet("idoinfo/{nombre}")]
        public async Task<IActionResult> IdoInfo(
            string            nombre,
            CancellationToken ct = default)
        {
            if (!_env.IsDevelopment())
                return NotFound();

            var resultado = await _ido.IdoInfoAsync(nombre, ct);
            return Ok(resultado);
        }

        /// <summary>
        /// Consulta genérica a cualquier IDO.
        /// GET /api/infor/ido/{nombre}?props=...&amp;filter=...&amp;recordCap=5
        /// </summary>
        [HttpGet("ido/{nombre}")]
        public async Task<IActionResult> ConsultarIdo(
            string              nombre,
            [FromQuery] string? props     = null,
            [FromQuery] string? filter    = null,
            [FromQuery] int     recordCap = 5,
            CancellationToken   ct        = default)
        {
            if (!_env.IsDevelopment())
                return NotFound();

            var resultado = await _ido.LoadAsync(
                ido:       nombre,
                props:     props,
                filter:    filter,
                recordCap: recordCap,
                ct:        ct);

            return Ok(resultado);
        }
    }
}
