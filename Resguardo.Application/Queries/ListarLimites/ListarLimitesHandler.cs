using Resguardo.Application.Interfaces;
using Resguardo.Application.Queries.ListarDepartamento;

namespace Resguardo.Application.Queries.ListarLimites
{
    public class ListarLimitesHandler
    {
        private readonly ILimiteQueryService _query;
        private readonly ListarDptoHandler _dptos;
        public ListarLimitesHandler(ILimiteQueryService query, ListarDptoHandler dptos)
        {
            _query = query;
            _dptos = dptos;
        }
        public async Task<IEnumerable<ListarLimitesResponse>> Ejecutar(ListarLimitesQuery filtro)
        {
            var dptos = await _dptos.Ejecutar();
            var configs = await _query.Listar(filtro, dptos);
            return configs;
        }
    }
}