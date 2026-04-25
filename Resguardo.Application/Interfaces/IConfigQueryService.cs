using Resguardo.Application.Queries.ListarConfig;
using Resguardo.Application.Queries.ListarDepartamento;

namespace Resguardo.Application.Interfaces
{
    public interface IConfigQueryService
    {
        Task<IEnumerable<ListarConfigResponse>> Listar(ListarConfigQuery filtro, IEnumerable<ListarDptoResponse> dptos);
    }
}
