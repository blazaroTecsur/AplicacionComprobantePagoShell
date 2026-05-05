using Microsoft.EntityFrameworkCore;
using Resguardo.Application.Common;
using Resguardo.Application.Interfaces;
using Resguardo.Application.Queries.ListarEfectivos;
using Resguardo.Infrastructure.Data;

namespace Resguardo.Infrastructure.QueryServices
{
    public class EfectivoQueryService : IEfectivoQueryService
    {
        private readonly DBContexto _contexto;
        public EfectivoQueryService(DBContexto contexto)
        {
            _contexto = contexto;
        }
        public async Task<IEnumerable<ListarEfectivoResponse>> Listar(int idServicioProv)
        {
            var efectivos = await _contexto.Efectivo
                .Where(s => s.IdServicioProv == idServicioProv)
                .Select(g => new ListarEfectivoResponse
                {
                    IdEfectivo = g.Id,
                    Dni = g.PersonalNav.Dni,
                    Nombres = g.PersonalNav.Nombres,
                    Apellidos = g.PersonalNav.Apellidos,
                    Telefono = g.Telefono,
                    HraInicio = g.HraInicio,
                    HraFinal = g.HraFinal,
                    Asistio = g.Asistio,
                    SroRefencia = g.SroRefencia,
                    Comentario = g.Comentario,
                    HraAmplia = g.HraAmplia,
                    EstAmplia = g.EstAmplia,
                    UsuarioApro = g.UsuarioApro,
                    FechaApro = g.FechaApro,
                    ComentApro = g.ComentApro,
                    Ampliar = string.IsNullOrEmpty(g.EstAmplia) || g.EstAmplia == Constantes.AMPL_PENDIENTE,
                    Aprobar = g.EstAmplia == Constantes.AMPL_PENDIENTE
                }).ToListAsync();

            return efectivos;
        }        
    }
}