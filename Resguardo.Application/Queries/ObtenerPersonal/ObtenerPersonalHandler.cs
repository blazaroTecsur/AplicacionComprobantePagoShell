using Resguardo.Application.Interfaces;

namespace Resguardo.Application.Queries.ObtenerPersonal
{
    public class ObtenerPersonalHandler
    {
        private readonly IPersonalQueryService _query;
        public ObtenerPersonalHandler(IPersonalQueryService query)
        {
            _query = query;
        }
        public async Task<ObtenerPersonalResponse?> Ejecutar(string dni)
        {
            var personal = await _query.Obtener(dni);            
            return personal;
        }
    }
}