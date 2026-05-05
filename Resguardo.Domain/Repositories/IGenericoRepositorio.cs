using Resguardo.Domain.Entities;

namespace Resguardo.Domain.Interfaces
{
    public interface IGenericoRepositorio : IRepositorioBase<Generico>
    {
        Task<Generico?> Obtener(string tipo, string codigo);
        Task<IEnumerable<Generico?>> Obtener(string tipo, string[] codigos);
    }
}