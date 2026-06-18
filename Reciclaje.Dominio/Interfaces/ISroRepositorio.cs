using Reciclaje.Dominio.Entidades;

namespace Reciclaje.Dominio.Interfaces
{
    public interface ISroRepositorio
    {
        Task<IEnumerable<Srolinea>> BuscarLineas(
            string? numeroSro,
            DateTime? fechaTransaccion,
            string? articuloNoInv,
            string? descripcionArticulo);

        Task<Srolinea?> ObtenerLineaPorId(int srolineaId);

        Task ActualizarEstadoLinea(int srolineaId, string estadoLinea, string usuarioModificacion);

    }
}
