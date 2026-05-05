using Resguardo.Domain.Entities;

namespace Resguardo.Domain.Interfaces
{
    public interface IRepositorioBase<T> where T : EntidadBase
    {
        IQueryable<T> Consulta();
        Task<T> ObtenerPorId(int id);
        Task Insertar(T entidad);
        Task Actualizar(T entidad);
        Task Eliminar(int id);
    }
}
