using Resguardo.Application.Interfaces;
using Resguardo.Application.Services;

namespace Resguardo.Application.Queries.ListarConfig
{
    public class ListarConfigHandler
    {
        private readonly IConfigQueryService _query;
        private readonly IMaestroService _maestro;
        public ListarConfigHandler(IConfigQueryService query, IMaestroService maestro)
        {
            _query = query;
            _maestro = maestro;
        }
        public async Task<IEnumerable<ListarConfigResponse>> Ejecutar(ListarConfigQuery filtro)
        {
            var dptos = await _maestro.ListarDepartamento();
            var configs = await _query.Listar(filtro, dptos);
            return configs;
        }
    }
}
