using ComprobantePago.Application.DTOs.Comprobante.Requests;
using ComprobantePago.Application.DTOs.Comprobante.Response;
using ComprobantePago.Application.DTOs.Responses;

namespace ComprobantePago.Application.Interfaces.QueryServices
{
    public interface IComprobanteQueryService
    {
        // ── Consultas Comprobante ─────────────────
        Task<IEnumerable<ComprobanteDto>> BuscarAsync(BuscarComprobanteDto filtros);
        Task<ComprobanteDetalleDto> ObtenerDetalleAsync(string folio);
        Task<byte[]?> ObtenerPdfAsync(string folio);
        Task<IEnumerable<DocumentoElectronicoDto>> ObtenerDocumentosElectronicosAsync(string folio);

        // ── Consultas Imputación ──────────────────
        Task<IEnumerable<ImputacionDetalleDto>> ObtenerImputacionesAsync(string folio);
        Task<byte[]> ObtenerPlantillaImputacionAsync();
    }
}
