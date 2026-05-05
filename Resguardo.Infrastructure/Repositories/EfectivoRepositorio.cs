using Resguardo.Application.Common;
using Resguardo.Domain.Entities;
using Resguardo.Domain.Interfaces;
using Resguardo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Resguardo.Infrastructure.Repositorios
{
    public class EfectivoRepositorio(DBContexto contexto) : RepositorioBase<Efectivo>(contexto), IEfectivoRepositorio
    {
        public async Task<Efectivo?> Obtener(int id)
        {
            var efectivo = await _entidades
                        .Include(ef => ef.ServicioProvNav)
                            .ThenInclude(sp => sp.ServicioNav)
                        .FirstOrDefaultAsync(ef => ef.Id == id);
            return efectivo;
        }
        public async Task<IEnumerable<Efectivo>> ListarEfectivos(string[] dnis, DateOnly fecha)
        {
            var efectivos = await _entidades
                .AsNoTracking()
                .Where(ef => dnis.Contains(ef.PersonalNav.Dni) &&
                             ef.ServicioProvNav.ServicioNav.Fecha >= fecha.AddDays(-1) &&
                             ef.ServicioProvNav.ServicioNav.Fecha <= fecha.AddDays(+1) &&
                             ef.ServicioProvNav.Estado != Constantes.SERVPROV_CERRADO)
                .Select(ef => new Efectivo()
                {
                    ServicioProvNav = new ServicioProv()
                    {
                        ServicioNav = new Servicio()
                        {
                            Fecha = ef.ServicioProvNav.ServicioNav.Fecha,
                            HraInicio = ef.ServicioProvNav.ServicioNav.HraInicio,
                            HraFinal = ef.ServicioProvNav.ServicioNav.HraFinal,
                            DiaSig = ef.ServicioProvNav.ServicioNav.DiaSig
                        }
                    },
                })
                .ToListAsync();

            return efectivos;
        }
    }
}