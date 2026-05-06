using Microsoft.EntityFrameworkCore;
using Resguardo.Application.Common;
using Resguardo.Application.Interfaces;
using Resguardo.Application.Queries.ConsultarSolicitud;
using Resguardo.Application.Queries.ObtenerSolicitud;
using Resguardo.Application.Queries.ObtenerSolicitudFolio;
using Resguardo.Infrastructure.Data;

namespace Resguardo.Infrastructure.QueryServices
{
    public class SolicitudQueryService : ISolicitudQueryService
    {
        private readonly DBContexto _contexto;
        public SolicitudQueryService(DBContexto contexto)
        {
            _contexto = contexto;
        }
        public async Task<ObtenerSolicitudResponse?> Obtener(int id)
        {
            var solicitud = await _contexto.Solicitud.Where(s => s.Id == id).Select(g => new ObtenerSolicitudResponse
            {
                Id = g.Id,
                Folio = g.Folio,
                IdEstado = g.IdEstado,
                IdFlujo = g.IdFlujo,
                NumSro = g.NumSro,
                CodDpto = g.CodDpto,
                NomDpto = g.NomDpto,
                CodActv = g.CodActv,
                NomActv = g.NomActv,
                CodCapataz = g.CodCapataz,
                NomCapataz = g.NomCapataz,
                RucSctta = g.RucSctta,
                NomSctta = g.NomSctta,
                Celular = g.Celular,
                TpoTrabajo = g.TpoTrabajo,
                Coordenada = g.Coordenada,
                Direccion = g.Direccion,
                Servicios = g.Servicios.Select(s => new ObtenerServicioResponse
                {
                    Id = s.Id,
                    IdTpoServicio = s.IdTpoServicio,
                    TpoServicio = s.TpoServicioNav.Descripcion,
                    Fecha = s.Fecha,
                    HraInicio = s.HraInicio,
                    HraFinal = s.HraFinal,
                    DiaSig = s.DiaSig,
                    Coordenada = s.Coordenada,
                    Direccion = s.Direccion,
                    Cantidad = s.Cantidad,
                    CantidadBck = s.CantidadBck
                }).ToList()
            }).FirstOrDefaultAsync();

            return solicitud;
        }
        public async Task<ObtenerSolicitudFolioResponse?> Obtener(string folio)
        {
            var solicitud = await _contexto.Solicitud.Where(s => s.Folio == folio).Select(g => new ObtenerSolicitudFolioResponse
            {
                Folio = g.Folio,
                Estado = $"{g.EstadoNav.Codigo} - {g.EstadoNav.Descripcion}",
                Flujo = $"{g.FlujoNav.Codigo} - {g.FlujoNav.Descripcion}",
                NumSro = g.NumSro,
                Dpto = $"{g.CodDpto} - {g.NomDpto}",
                Actv = $"{g.CodActv} - {g.NomActv}",
                Capataz = $"{g.CodCapataz} - {g.NomCapataz}",
                Sctta = $"{g.RucSctta} - {g.NomSctta}",
                Celular = g.Celular,
                TpoTrabajo = g.TpoTrabajo
            }).FirstOrDefaultAsync();

            return solicitud;
        }
        public async Task<GridResponse<ConsultarSolicitudResponse>> Consultar(GridRequest<ConsultarSolicitudQuery> grid)
        {
            var consulta = _contexto.Solicitud.AsNoTracking().AsQueryable();
            var solicitud = grid.Filtros;

            if (!string.IsNullOrEmpty(solicitud.Folio))
                consulta = consulta.Where(x => x.Folio == solicitud.Folio);
            if (solicitud.IdEstado.HasValue)
                consulta = consulta.Where(x => x.IdEstado == solicitud.IdEstado);
            if (solicitud.IdFlujo.HasValue)
                consulta = consulta.Where(x => x.IdFlujo == solicitud.IdFlujo);
            if (!string.IsNullOrEmpty(solicitud.NumSro))
                consulta = consulta.Where(x => x.NumSro == solicitud.NumSro);

            var total = consulta.Count();

            var data = await consulta
                .OrderByDescending(x => x.FechaReg)
                .Skip((grid.Page - 1) * grid.PageSize)
                .Take(grid.PageSize)
                .Select(x => new ConsultarSolicitudResponse
                {
                    Id = x.Id,
                    Folio = x.Folio,
                    Tipo = x.TipoNav.Descripcion,
                    Estado = x.EstadoNav.Descripcion,
                    Modifica = x.EstadoNav.Codigo == Constantes.COD_ESTADO_INGRESADO ? true : false,
                    Aprueba = x.EstadoNav.Codigo == Constantes.COD_ESTADO_INGRESADO ? true : false,
                    Confirma = x.EstadoNav.Codigo == Constantes.COD_ESTADO_APROBJEFE ? true : false,
                    Edita = x.EstadoNav.Codigo == Constantes.COD_ESTADO_APROBJEFE ? true : false,
                    Flujo = x.FlujoNav.Descripcion,
                    NumSro = x.NumSro,
                    Sctta = $"{x.RucSctta} - {x.NomSctta}",
                    UsuarioReg = x.UsuarioReg,
                    FechaReg = x.FechaReg,
                    HrasSolicitadas = "02:00:00",
                    FolioRef = x.FolioRef
                })
                .ToListAsync();

            return new GridResponse<ConsultarSolicitudResponse>
            {
                Data = data,
                Total = total
            };
        }
    }
}
