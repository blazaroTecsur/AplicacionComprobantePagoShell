using Microsoft.EntityFrameworkCore;
using Resguardo.Domain.Entities;
using Resguardo.Domain.Repositories;
using Resguardo.Infrastructure.Data;
using Resguardo.Infrastructure.Repositorios;

namespace Resguardo.Infrastructure.Repositories
{
    public class ServicioProvRepositorio(DBContexto contexto) : RepositorioBase<ServicioProv>(contexto), IServicioProvRepositorio
    {
        public async Task<ServicioProv> Obtener(int id)
        {
            var servicioProv = await _entidades.Include(s => s.Efectivos).Where(s => s.Id == id).FirstAsync();
            return servicioProv;
        }
    }
}
