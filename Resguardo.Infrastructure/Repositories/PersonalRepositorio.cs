using Microsoft.EntityFrameworkCore;
using Resguardo.Domain.Entities;
using Resguardo.Domain.Repositories;
using Resguardo.Infrastructure.Data;
using Resguardo.Infrastructure.Repositorios;

namespace Resguardo.Infrastructure.Repositories
{
    public class PersonalRepositorio(DBContexto contexto) : RepositorioBase<Personal>(contexto), IPersonalRepositorio
    {
        public async Task<Personal?> ObtenerPorDni(string dni)
        {
            var personal = await _entidades
                .Where(g => g.Dni == dni)
                .FirstOrDefaultAsync();

            return personal;
        }
    }
}
