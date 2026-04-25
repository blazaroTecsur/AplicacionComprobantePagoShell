using Resguardo.Application.Common;
using Resguardo.Application.Queries.ConsultarCapataz;
using Resguardo.Application.Queries.ListarDepartamento;

namespace Resguardo.Application.Services
{
    public interface IMaestroService
    {
        Task<GridResponse<ConsultarCapatazResponse>> ConsultarCapataz(GridRequest<ConsultarCapatazQuery> grid);
        Task<IEnumerable<ListarDptoResponse>> ListarDepartamento();
    }
}
