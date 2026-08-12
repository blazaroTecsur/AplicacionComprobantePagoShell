using Resguardo.Domain.Entities;
using Resguardo.Domain.Interfaces;
using Resguardo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Resguardo.Application.Common;

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
            string[] estados = { Constantes.COD_ESTADO_APROBJEFE, Constantes.COD_ESTADO_APROBCLIENT };
            var servicios = await _entidades
                .Where(s => s.SolicitudNav.CodDpto == codDpto && 
                            s.IdTpoServicio == idTpoServicio && 
                            s.Fecha == fecha && 
                            s.Turno == turno &&
                            estados.Contains(s.SolicitudNav.EstadoNav.Codigo))
                .SumAsync(s => s.Cantidad);
            return servicios;
        }        
    }
}