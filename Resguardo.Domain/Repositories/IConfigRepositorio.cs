using Resguardo.Domain.Entities;
using Resguardo.Domain.Interfaces;

namespace Resguardo.Domain.Repositories
{
    public interface IConfigRepositorio : IRepositorioBase<Config>
    {
        Task<IEnumerable<Config>> Listar(DateOnly fecha);
        Task<int> ObtenerDiurno(string codDpto, int idTpoServicio, DateOnly fecha);
        Task<int> ObtenerNocturno(string codDpto, int idTpoServicio, DateOnly fecha);
    }
}
