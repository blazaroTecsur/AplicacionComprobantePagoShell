using Resguardo.Domain.Entities;
using Resguardo.Domain.Interfaces;
using Resguardo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Resguardo.Infrastructure.Repositorios
{
    public class ServicioRepositorio(DBContexto contexto) : RepositorioBase<Servicio>(contexto), IServicioRepositorio
    {
        public async Task<IEnumerable<Servicio>> Listar(int idSolicitud)
        {
            var servicios = await _entidades
                .Include(s => s.ServicioProvs)
                .Where(s => s.IdSolicitud == idSolicitud)
                .ToListAsync();
            return servicios;
        }
        public async Task<int> CalularCantidad(string codDpto, int idTpoServicio, DateOnly fecha, string turno)
        {
            var servicios = await _entidades
                .Where(s => s.SolicitudNav.CodDpto == codDpto && s.IdTpoServicio == idTpoServicio && s.Fecha == fecha && s.Turno == turno)
                .SumAsync(s => s.Cantidad);
            return servicios;
        }
    }
}