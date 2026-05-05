using Resguardo.Application.Common;
using Resguardo.Application.Queries.ConsultarServicio;
using Resguardo.Application.Queries.ListarServicio;

namespace Resguardo.Application.Interfaces
{
    public interface IServicioQueryService
    {
        Task<IEnumerable<ListarServicioResponse>> Listar(int idSolicitud);
        Task<GridResponse<ConsultarServicioResponse>> Consultar(GridRequest<ConsultarServicioQuery> grid);        
    }
}