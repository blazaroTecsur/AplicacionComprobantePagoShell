using Microsoft.EntityFrameworkCore;
using Resguardo.Domain.Entities;
using Resguardo.Domain.Interfaces;
using Resguardo.Infrastructure.Data;

namespace Resguardo.Infrastructure.Repositorios
{
    public class SolicitudRepositorio(DBContexto contexto) : RepositorioBase<Solicitud>(contexto), ISolicitudRepositorio
    {
        public async Task<Solicitud> Obtener(int id)
        {
            var solicitud = await _entidades
                .Include(s => s.Servicios)
                .Include(s => s.EstadoNav)
                .Include(s => s.FlujoNav)
                .Where(s => s.Id == id).FirstAsync();
            return solicitud;
        }
    }
}
