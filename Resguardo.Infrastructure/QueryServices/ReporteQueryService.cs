using Microsoft.EntityFrameworkCore;
using Resguardo.Application.Interfaces;
using Resguardo.Application.Queries.ReporteEfectivo;
using Resguardo.Application.Queries.ReporteSolicitud;
using Resguardo.Infrastructure.Data;

namespace Resguardo.Infrastructure.QueryServices
{
    public class ReporteQueryService : IReporteQueryService
    {
        private readonly DBContexto _contexto;
        public ReporteQueryService(DBContexto contexto)
        {
            _contexto = contexto;
        }
        public async Task<IEnumerable<ReporteSolicitudResponse>> ReporteSolicitudes(ReporteSolicitudQuery filtro)
        {
            var consulta = _contexto.Servicio
                .AsNoTracking()
                .Where(x => x.Fecha >= filtro.FechaDsd && x.Fecha <= filtro.FechaHst)
                .AsQueryable();

            if (filtro.IdFlujo.HasValue)
                consulta = consulta.Where(x => x.SolicitudNav.IdFlujo == filtro.IdFlujo);

            var reporte = await consulta
                    .Select(x => new ReporteSolicitudResponse
                    {
                        FechaReg = x.SolicitudNav.FechaReg,
                        Flujo = x.SolicitudNav.FlujoNav.Descripcion,
                        Folio = x.Folio,
                        NumSro = x.SolicitudNav.NumSro,
                        Estado = x.SolicitudNav.EstadoNav.Descripcion,
                        RucSctta = x.SolicitudNav.RucSctta,
                        NomSctta = x.SolicitudNav.NomSctta,
                        NomSupr = x.SolicitudNav.NomSupr,
                        NomCapataz = x.SolicitudNav.NomCapataz,
                        Celular = x.SolicitudNav.Celular,
                        TpoTrabajo = x.SolicitudNav.TpoTrabajo,
                        CodDpto = x.SolicitudNav.CodDpto,
                        NomDpto = x.SolicitudNav.NomDpto,
                        UsuarioApro = x.SolicitudNav.UsuarioApro,
                        FechaApro = x.SolicitudNav.FechaApro,
                        TipoServicio = x.TpoServicioNav.Descripcion,
                        Fecha = x.Fecha,
                        HraInicio = x.HraInicio,
                        HraFinal = x.HraFinal,
                        Turno = x.Turno,
                        Cantidad = x.Cantidad,
                        Direccion = x.Direccion,
                        Coordenada = x.Coordenada,
                        Comentario = x.Comentario
                    }).ToListAsync();

            return reporte;
        }
        public async Task<IEnumerable<ReporteEfectivoResponse>> ReporteEfectivos(ReporteEfectivoQuery filtro)
        {
            var consulta = _contexto.Efectivo
               .AsNoTracking()
               .Where(x =>
               x.ServicioProvNav.ServicioNav.Fecha >= filtro.FechaDsd &&
               x.ServicioProvNav.ServicioNav.Fecha <= filtro.FechaHst)
               .AsQueryable();

            if (filtro.IdServicio.HasValue)
                consulta = consulta.Where(x => x.ServicioProvNav.ServicioNav.IdTpoServicio == filtro.IdServicio);

            var reporte = await consulta
                    .Select(x => new ReporteEfectivoResponse
                    {
                        Fecha = x.ServicioProvNav.ServicioNav.Fecha,
                        Nombres = x.PersonalNav.Nombres,
                        Telefono = x.Telefono,
                        Direccion = x.ServicioProvNav.ServicioNav.Direccion,
                        Coordenada = x.ServicioProvNav.ServicioNav.Coordenada,
                        TipoServicio = x.ServicioProvNav.ServicioNav.TpoServicioNav.Descripcion,
                        HraInicio = x.ServicioProvNav.ServicioNav.HraInicio,
                        HraFinal = x.ServicioProvNav.ServicioNav.HraFinal,
                        HraInicioReal = x.HraInicio,
                        HraFinalReal = x.HraFinal,
                        HraAmplia = x.HraAmplia,
                        UsuarioReg = x.ServicioProvNav.ServicioNav.SolicitudNav.UsuarioReg,
                        CodDpto = x.ServicioProvNav.ServicioNav.SolicitudNav.CodDpto,
                        NomDpto = x.ServicioProvNav.ServicioNav.SolicitudNav.NomDpto,
                        NomActv = x.ServicioProvNav.ServicioNav.SolicitudNav.NomActv,
                        NomSctta = x.ServicioProvNav.ServicioNav.SolicitudNav.NomSctta,
                        NomCapataz = x.ServicioProvNav.ServicioNav.SolicitudNav.NomCapataz,
                        Celular = x.ServicioProvNav.ServicioNav.SolicitudNav.Celular,
                        NumSro = x.ServicioProvNav.ServicioNav.SolicitudNav.NumSro,
                        Folio = x.ServicioProvNav.ServicioNav.Folio
                    })
                    .ToListAsync();

            return reporte;
        }
    }
}
