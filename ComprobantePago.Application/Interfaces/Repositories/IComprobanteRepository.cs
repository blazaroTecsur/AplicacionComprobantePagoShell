using ComprobantePago.Application.Commands.Comprobante;
using ComprobantePago.Application.Commands.Imputacion;
using ComprobantePago.Application.DTOs.Comprobante.Response;
using ComprobantePago.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace ComprobantePago.Application.Interfaces.Repositories
{
    public interface IComprobanteRepository
    {
        // ── Comprobante ───────────────────────────
        Task<string> GuardarAsync(
            RegistrarComprobanteCommand command);
        Task EnviarAsync(
            EnviarComprobanteCommand command);
        Task FirmarAsync(
            FirmarComprobanteCommand command);
        Task AprobarAsync(
            AprobarComprobanteCommand command);
        Task AnularAsync(
            AnularComprobanteCommand command);
        Task DerivarAsync(
            DerivarComprobanteCommand command);

        // ── Imputación ────────────────────────────
        Task<ImputacionDetalleDto> AgregarImputacionAsync(
            AgregarImputacionCommand command);
        Task<ImputacionDetalleDto> EditarImputacionAsync(
            EditarImputacionCommand command);
        Task EliminarImputacionAsync(
            EliminarImputacionCommand command);
        Task<IEnumerable<ImputacionDetalleDto>> CargarImputacionMasivaAsync(
            IFormFile file);
        Task<ValidacionSunatDto> ValidarXmlSunatAsync(IFormFile archivo);
        Task<ValidacionSunatDto> ValidarPdfSunatAsync(IFormFile archivo);
        Task<ValidacionSunatDto> ValidarZipSunatAsync(IFormFile archivo);
        Task GuardarDocumentosAsync(string folio, List<(byte[] contenido, string nombre, string tipo, string subTipo)> archivos);
        Task<Domain.Entities.DocumentoElectronico?> DescargarDocumentoAsync(int idDocumento);
        Task EliminarDocumentoAsync(int idDocumento);
        Task<string> GenerarFolioAsync();
    }
}
