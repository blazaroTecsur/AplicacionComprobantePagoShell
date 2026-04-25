using Resguardo.Domain.Entities;
using Resguardo.Domain.Interfaces;

namespace Resguardo.Domain.Repositories
{
    public interface IPersonalRepositorio : IRepositorioBase<Personal>
    {
        Task<Personal?> ObtenerPorDni(string dni);
    }
}
