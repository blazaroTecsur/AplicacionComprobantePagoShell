using Resguardo.Domain.Entities;

namespace Resguardo.Domain.Interfaces
{
    public interface IEfectivoRepositorio : IRepositorioBase<Efectivo>
    {
        public Task<Efectivo?> Obtener(int id);
        public Task<IEnumerable<Efectivo>> ListarEfectivos(string[] dnis, DateOnly fecha);
    }
}
