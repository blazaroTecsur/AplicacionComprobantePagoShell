using Resguardo.Domain.Entities;

namespace Resguardo.Domain.Interfaces
{
    public interface IServicioRepositorio : IRepositorioBase<Servicio>
    {
        Task<IEnumerable<Servicio>> Listar(int idSolicitud);
        Task<int> CalularCantidad(string codDpto, int idTpoServicio, DateOnly fecha, string turno);
    }
}
