using Reciclaje.Aplicacion.DTOs.Reciclaje;

namespace Reciclaje.Aplicacion.Interfaces
{
    /// <summary>
    /// Genera el reporte Excel "Vale de Recupero de la SST — Confirmación"
    /// agrupado por SRO.
    /// </summary>
    public interface IValeConfirmacionReporteServicio
    {
        /// <summary>
        /// Genera el archivo .xlsx en memoria y devuelve los bytes
        /// listos para ser descargados desde el controlador.
        /// </summary>
        Task<byte[]> GenerarExcel(ValeRecuperoBuscarDto filtros);
    }
}
