using Resguardo.Application.Interfaces;
using Resguardo.Application.Services;

namespace Resguardo.Application.Queries.ListarLimites
{
    public class ListarLimitesHandler
    {
        private readonly ILimiteQueryService _query;
        private readonly IMaestroService _maestro;
        public ListarLimitesHandler(ILimiteQueryService query, IMaestroService maestro)
        {
            _query = query;
            _maestro = maestro;
        }
        public async Task<IEnumerable<ListarLimitesResponse>> Ejecutar(ListarLimitesQuery filtro)
        {
            var dptos = await _maestro.ListarDepartamento();
            var configs = await _query.Listar(filtro, dptos);
            return configs;
        }
    }
}
