using Resguardo.Application.Interfaces;

namespace Resguardo.Application.Queries.ListarEfectivos
{
    public class ListarEfectivoHandler
    {
        private readonly IEfectivoQueryService _query;
        public ListarEfectivoHandler(IEfectivoQueryService query)
        {
            _query = query;
        }
        public async Task<IEnumerable<ListarEfectivoResponse>> Ejecutar(int idServicioProv)
        {
            return await _query.Listar(idServicioProv);
        }
    }
}
