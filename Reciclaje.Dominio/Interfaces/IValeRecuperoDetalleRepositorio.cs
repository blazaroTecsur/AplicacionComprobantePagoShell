using Reciclaje.Dominio.Entidades;

namespace Reciclaje.Dominio.Interfaces;

public interface IValeRecuperoDetalleRepositorio
{
    Task InsertarDetalles(IEnumerable<Valerecuperodetalle> detalles);

    Task<IEnumerable<Valerecuperodetalle>> ObtenerPorValeId(int valeId);

    /// <summary>
    /// Devuelve todos los detalles cuyo vale padre coincida con los filtros
    /// de búsqueda (NumeroSro, NumeroVale, ArticuloReciclaje, FechaVale).
    /// Incluye la navegación a Valerecupero → Srolinea → Sro.
    /// </summary>
    Task<IEnumerable<Valerecuperodetalle>> BuscarParaReporte(
        string? numeroSro,
        string? numeroVale,
        string? codigoArticuloReciclaje,
        string? descripcionArticuloReciclaje,
        DateTime? fechaVale);

    /// <summary>
    /// Distribuye CantidadRecibida (llenado secuencial por CantidadNoInv desc)
    /// y PesoRecibido (mismo valor en todas las líneas) entre los detalles del vale.
    /// </summary>
    Task DistribuirRecepcion(
        int valeId,
        decimal cantidadRecibida,
        decimal pesoRecibido,
        bool checkRecepcion,
        string usuarioModificacion);
}
