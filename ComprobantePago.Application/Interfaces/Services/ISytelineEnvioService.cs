using ComprobantePago.Application.DTOs.Responses;

namespace ComprobantePago.Application.Interfaces.Services
{
    /// <summary>
    /// Envía comprobantes aprobados al IDO SLAptrxs/SLAptrxds de Infor Syteline.
    /// </summary>
    public interface ISytelineEnvioService
    {
        /// <summary>
        /// Inserta una cabecera de comprobante en SLAptrxs.
        /// Devuelve el voucher asignado y el ItemId interno de Syteline (necesario para rollback).
        /// </summary>
        Task<(int Voucher, string ItemId)> EnviarCabeceraAsync(
            SytelineCabeceraDto cabecera,
            CancellationToken ct = default);

        /// <summary>
        /// Inserta múltiples cabeceras en SLAptrxs en una sola operación.
        /// </summary>
        Task<IEnumerable<int>> EnviarCabecerasAsync(
            IEnumerable<SytelineCabeceraDto> cabeceras,
            CancellationToken ct = default);

        /// <summary>
        /// Elimina una cabecera de SLAptrxs usando el ItemId devuelto por el insert.
        /// Usar como rollback cuando la inserción de distribución falla.
        /// </summary>
        Task EliminarCabeceraAsync(string itemId, CancellationToken ct = default);

        /// <summary>
        /// Devuelve el siguiente número de voucher disponible en SLAptrxs (max actual + 1).
        /// Usar para pre-asignar vouchers antes de exportar el Excel.
        /// </summary>
        Task<int> ObtenerSiguienteVoucherAsync(CancellationToken ct = default);

        /// <summary>
        /// Inserta las líneas de distribución (SLAptrxds) para un comprobante ya registrado.
        /// Genera: línea(s) de gasto, línea NR (si hay IGV), línea IGV y línea exento (si aplica).
        /// </summary>
        Task EnviarDistribucionAsync(
            SytelineCabeceraDto cabecera,
            IEnumerable<SytelineDistribucionDto> lineas,
            int voucher,
            CancellationToken ct = default);
    }
}
