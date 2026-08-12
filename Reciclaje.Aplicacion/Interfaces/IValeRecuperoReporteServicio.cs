using Reciclaje.Aplicacion.DTOs.Reciclaje;

namespace Reciclaje.Aplicacion.Interfaces
{
    /// <summary>
    /// Servicio que genera el reporte PDF "Vale de Recupero de la SST"
    /// a partir de los filtros de búsqueda actuales.
    /// </summary>
    public interface IValeRecuperoReporteServicio
    {
        /// <summary>
        /// Construye el <see cref="ValeRecuperoReporteDto"/> con los datos
        /// necesarios para renderizar el PDF.
        /// </summary>
        Task<ValeRecuperoReporteDto> ObtenerDatosReporte(ValeRecuperoBuscarDto filtros);

        /// <summary>
        /// Genera el PDF en memoria y devuelve el arreglo de bytes listo
        /// para ser descargado desde el controlador.
        /// </summary>
        Task<byte[]> GenerarPdf(ValeRecuperoBuscarDto filtros);
    }
}
