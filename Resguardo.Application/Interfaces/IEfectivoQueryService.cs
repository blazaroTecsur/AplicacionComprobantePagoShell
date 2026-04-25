using Resguardo.Application.Queries.ListarEfectivos;

namespace Resguardo.Application.Interfaces
{
    public interface IEfectivoQueryService
    {
        public Task<IEnumerable<ListarEfectivoResponse>> Listar(int idServicioProv);
    }
}
