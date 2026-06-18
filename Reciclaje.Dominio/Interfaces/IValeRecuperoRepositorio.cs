using Reciclaje.Dominio.Entidades;

namespace Reciclaje.Dominio.Interfaces
{
    public interface IValeRecuperoRepositorio
    {
        Task<string> GenerarNumeroVale();
        Task InsertarVale(Valerecupero vale);
        Task InsertarVales(IEnumerable<Valerecupero> vales);
        Task<IEnumerable<Valerecupero>> Buscar(
            string? numeroSro,
            string? numeroVale,
            string? codigoArticuloReciclaje,
            string? descripcionArticuloReciclaje,
            DateTime? fechaVale);
        Task<Valerecupero?> ObtenerPorId(int valeId);
        Task Actualizar(Valerecupero vale);
    }
}