using ComprobantePago.Application.DTOs.Comprobante.Common;

namespace ComprobantePago.Application.Interfaces.Services.Maestros
{
    /// <summary>
    /// Servicio para catálogos de códigos de unidad (1, 3 y 4).
    /// </summary>
    public interface ICatalogoUnidadService
    {
        /// <param name="unidad">1, 3 o 4</param>
        Task<IEnumerable<ComboDto>> ObtenerCodigosUnidadAsync(int unidad, string filtro = "");
    }
}
