using Resguardo.Application.Interfaces;

namespace Resguardo.Application.Queries.ObtenerConfig
{
    public class ObtenerLimitesHandler
    {
        private readonly ILimiteQueryService _query;        
        public ObtenerLimitesHandler(ILimiteQueryService query)
        {
            _query = query;
        }
        public async Task<IEnumerable<ObtenerLimitesResponse>> Ejecutar(ObtenerLimitesQuery filtro)
        {            
            var configs = await _query.Obtener(filtro);
            return configs;
        }
    }
}