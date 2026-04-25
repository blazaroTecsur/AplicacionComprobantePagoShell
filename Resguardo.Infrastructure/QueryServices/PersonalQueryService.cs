using Microsoft.EntityFrameworkCore;
using Resguardo.Application.Interfaces;
using Resguardo.Application.Queries.ObtenerPersonal;
using Resguardo.Infrastructure.Data;

namespace Resguardo.Infrastructure.QueryServices
{
    public class PersonalQueryService : IPersonalQueryService
    {
        private readonly DBContexto _contexto;
        public PersonalQueryService(DBContexto contexto)
        {
            _contexto = contexto;
        }
        public async Task<ObtenerPersonalResponse> Obtener(string dni)
        {
            var personal = await _contexto.Personal
                .Where(g => g.Dni == dni )
                .Select(g => new ObtenerPersonalResponse
                {
                    IdPersonal = g.Id,
                    Nombres = g.Nombres,
                    Apellidos = g.Apellidos,
                    Telefono = g.Telefono
                }).FirstOrDefaultAsync();

            return personal;
        }
    }
}
