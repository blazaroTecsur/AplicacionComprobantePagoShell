using Seguridad.Infrastructure.Handler.Authorization;
using ComprobantePago.Application.Commands.Comprobante;
using ComprobantePago.Application.Common;
using ComprobantePago.Application.DTOs.Comprobante.Requests;
using ComprobantePago.Application.Interfaces.QueryServices;
using ComprobantePago.Application.Interfaces.Repositories;
using ComprobantePago.Application.Interfaces.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComprobantePago.Web.Controllers
{
    [Authorize]
    [Permission("COMP.APROBAR")]
    [Route("Comprobante")]
    public class ComprobanteAprobarController : Controller
    {
        private readonly IComprobanteRepository          _repository;
        private readonly ISytelineQueryService           _sytelineService;
        private readonly ISytelineEnvioService           _sytelineEnvio;
        private readonly IValidator<AccionComprobanteDto> _accionValidator;
        private readonly ILogger<ComprobanteAprobarController> _logger;

        public ComprobanteAprobarController(
            IComprobanteRepository          repository,
            ISytelineQueryService           sytelineService,
            ISytelineEnvioService           sytelineEnvio,
            IValidator<AccionComprobanteDto> accionValidator,
            ILogger<ComprobanteAprobarController> logger)
        {
            _repository      = repository;
            _sytelineService = sytelineService;
            _sytelineEnvio   = sytelineEnvio;
            _accionValidator = accionValidator;
            _logger          = logger;
        }

        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Aprobar([FromBody] AprobarComprobanteCommand command)
        {
            var validacion = await _accionValidator.ValidateAsync(command.Comprobante);
            if (!validacion.IsValid)
                return BadRequest(new { errores = validacion.Errors.Select(e => e.ErrorMessage) });
            await _repository.AprobarAsync(command);
            return Ok(BaseResponse.Ok());
        }

        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnviarCabeceraASyteline(
            [FromBody] List<string> folios,
            CancellationToken ct)
        {
            if (folios is null || folios.Count == 0)
                return BadRequest(new { error = "Debe seleccionar al menos un comprobante." });

            var cabeceras  = await _sytelineService.ObtenerCabecerasSytelineAsync(folios);
            var resultados = new List<object>();
            var errores    = new List<object>();

            foreach (var cabecera in cabeceras)
            {
                string itemIdCabecera = string.Empty;
                try
                {
                    var (voucher, itemId) = await _sytelineEnvio.EnviarCabeceraAsync(cabecera, ct);
                    itemIdCabecera = itemId;

                    var distribuciones = await _sytelineService
                        .ObtenerDistribucionSytelineAsync(new List<string> { cabecera.Folio });

                    await _sytelineEnvio.EnviarDistribucionAsync(cabecera, distribuciones, voucher, ct);

                    await _repository.DerivarAsync(new DerivarComprobanteCommand
                    {
                        Comprobante     = new AccionComprobanteDto { Folio = cabecera.Folio },
                        VoucherSyteline = voucher
                    });

                    resultados.Add(new { folio = cabecera.Factura, voucher });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error al enviar comprobante {Factura} a Syteline.", cabecera.Factura);

                    if (!string.IsNullOrEmpty(itemIdCabecera))
                    {
                        _logger.LogWarning(
                            "Revirtiendo cabecera de {Factura} (ItemId={ItemId}) por fallo en distribución.",
                            cabecera.Factura, itemIdCabecera);
                        try { await _sytelineEnvio.EliminarCabeceraAsync(itemIdCabecera, ct); }
                        catch (Exception rollbackEx)
                        {
                            _logger.LogError(rollbackEx,
                                "No se pudo revertir la cabecera de {Factura}. Requiere limpieza manual en Syteline.",
                                cabecera.Factura);
                        }
                    }

                    errores.Add(new { folio = cabecera.Factura, error = ex.Message });
                }
            }

            return Ok(new
            {
                enviados = resultados.Count,
                errores  = errores.Count,
                detalle  = resultados,
                fallos   = errores
            });
        }
    }
}
