using Resguardo.Domain.Entities;

namespace Resguardo.Domain.Interfaces
{
    public interface ISolicitudRepositorio : IRepositorioBase<Solicitud>
    {
        public Task<Solicitud> Obtener(int id);
    }
}