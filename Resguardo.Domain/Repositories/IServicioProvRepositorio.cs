using Resguardo.Domain.Entities;
using Resguardo.Domain.Interfaces;

namespace Resguardo.Domain.Repositories
{
    public interface IServicioProvRepositorio : IRepositorioBase<ServicioProv>
    {
        Task<ServicioProv> Obtener(int id);
    }
}
