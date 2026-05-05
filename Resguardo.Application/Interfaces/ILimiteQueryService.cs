using Resguardo.Application.Queries.ListarDepartamento;
using Resguardo.Application.Queries.ListarLimites;
using Resguardo.Application.Queries.ObtenerConfig;

namespace Resguardo.Application.Interfaces
{
    public interface ILimiteQueryService
    {
        Task<IEnumerable<ListarLimitesResponse>> Listar(ListarLimitesQuery filtro, IEnumerable<ListarDptoResponse> dptos);
        Task<IEnumerable<ObtenerLimitesResponse>> Obtener(ObtenerLimitesQuery filtro);
    }
}
