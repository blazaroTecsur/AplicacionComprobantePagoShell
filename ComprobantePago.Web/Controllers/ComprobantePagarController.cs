using Seguridad.Infrastructure.Handler.Authorization;
using ComprobantePago.Application.Commands.Comprobante;
using ComprobantePago.Application.Common;
using ComprobantePago.Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seguridad.Abstractions.Interfaces;

namespace ComprobantePago.Web.Controllers
{
    [Authorize]
    [Permission("COMP.PAGAR")]
    [Route("Comprobante")]
    public class ComprobantePagarController : Controller
    {
        private readonly IComprobanteRepository _repository;
        private readonly IUsuarioContexto _usuario;
        private readonly ILogger<ComprobantePagarController> _logger;

        public ComprobantePagarController(
            IComprobanteRepository repository,
            IUsuarioContexto usuario,
            ILogger<ComprobantePagarController> logger)
        {
            _repository = repository;
            _usuario    = usuario;
            _logger     = logger;
        }

        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pagar([FromBody] PagarComprobanteCommand command)
        {
            var folio = command.Comprobante?.Folio;
            if (string.IsNullOrWhiteSpace(folio))
                return BadRequest(new { error = "Folio requerido." });

            await _repository.PagarAsync(folio, _usuario.Correo);
            _logger.LogInformation("Comprobante {Folio} marcado como PAGADO por {Usuario}",
                folio, _usuario.Correo);
            return Ok(BaseResponse.Ok());
        }
    }
}
