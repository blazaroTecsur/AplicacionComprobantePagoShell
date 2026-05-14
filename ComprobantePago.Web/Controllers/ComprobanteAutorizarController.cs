using Seguridad.Infrastructure.Handler.Authorization;
using ComprobantePago.Application.Commands.Comprobante;
using ComprobantePago.Application.Common;
using ComprobantePago.Application.DTOs.Comprobante.Requests;
using ComprobantePago.Application.Interfaces.Repositories;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seguridad.Abstractions.Interfaces;

namespace ComprobantePago.Web.Controllers
{
    [Authorize]
    [Permission("COMP.AUTORIZAR")]
    [Route("Comprobante")]
    public class ComprobanteAutorizarController : Controller
    {
        private readonly IComprobanteRepository           _repository;
        private readonly IValidator<AccionComprobanteDto> _accionValidator;
        private readonly IUsuarioContexto                 _usuario;
        private readonly ILogger<ComprobanteAutorizarController> _logger;

        public ComprobanteAutorizarController(
            IComprobanteRepository           repository,
            IValidator<AccionComprobanteDto> accionValidator,
            IUsuarioContexto                 usuario,
            ILogger<ComprobanteAutorizarController> logger)
        {
            _repository      = repository;
            _accionValidator = accionValidator;
            _usuario         = usuario;
            _logger          = logger;
        }

        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Firmar([FromBody] FirmarComprobanteCommand command)
        {
            var validacion = await _accionValidator.ValidateAsync(command.Comprobante);
            if (!validacion.IsValid)
                return BadRequest(new { errores = validacion.Errors.Select(e => e.ErrorMessage) });
            await _repository.FirmarAsync(command);
            return Ok(BaseResponse.Ok());
        }

        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Derivar([FromBody] DerivarComprobanteCommand command)
        {
            var validacion = await _accionValidator.ValidateAsync(command.Comprobante);
            if (!validacion.IsValid)
                return BadRequest(new { errores = validacion.Errors.Select(e => e.ErrorMessage) });
            await _repository.DerivarAsync(command);
            return Ok(BaseResponse.Ok());
        }

        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AutorizarMasivo([FromBody] List<string> folios)
        {
            if (folios is null || folios.Count == 0)
                return BadRequest(new { error = "Debe seleccionar al menos un comprobante." });

            await _repository.FirmarMasivoAsync(folios, _usuario.Correo);
            _logger.LogInformation("Autorización masiva de {Count} comprobantes por {Usuario}",
                folios.Count, _usuario.Correo);
            return Ok(BaseResponse.Ok($"Se autorizaron {folios.Count} comprobante(s)."));
        }
    }
}
