using Resguardo.Application.Queries.ListarServicio;

namespace Resguardo.Application.Interfaces
{
    public interface IServicioProvQueryService
    {
        Task<IEnumerable<ListarServicioProvItem>> Listar(int idSolicitud);
    }
}
