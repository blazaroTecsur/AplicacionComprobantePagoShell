using Microsoft.AspNetCore.Http;
using Resguardo.Application.Exceptions;
using Resguardo.Application.Interfaces;

namespace Resguardo.Application.Queries.ObtenerSolicitud
{
    public class ObtenerSolicitudHandler
    {
        private readonly ISolicitudQueryService _query;
        public ObtenerSolicitudHandler(ISolicitudQueryService query)
        {
            _query = query;
        }
        public async Task<ObtenerSolicitudResponse?> Ejecutar(int id)
        {
            var solicitud = await _query.Obtener(id);
            if (solicitud is null)
                throw new BusinessException(StatusCodes.Status400BadRequest.ToString(), $"La solicitud {id} no existe");

            double subHrasSolicitadas = 0, tolHrasSolicitadas = 0, tolHrasAprobadas = 0;
            DateTime fecha, fechaIni, fechaFin;
            foreach (var servicio in solicitud.Servicios)
            {
                if (!TimeSpan.TryParse(servicio.HraInicio, out var horInicio))
                    throw new BusinessException(StatusCodes.Status400BadRequest.ToString(), "La hora de inicio es inválida");
                if (!TimeSpan.TryParse(servicio.HraFinal, out var horFinal))
                    throw new BusinessException(StatusCodes.Status400BadRequest.ToString(), "La hora fin es inválida");

                subHrasSolicitadas = (horFinal - horInicio).TotalHours;
                if (horInicio > horFinal)
                {
                    fecha = servicio.Fecha.ToDateTime(TimeOnly.MinValue);
                    fechaIni = fecha.AddTicks(horInicio.Ticks);
                    fechaFin = fecha.AddDays(+1).AddTicks(horFinal.Ticks);
                    subHrasSolicitadas = fechaFin.Subtract(fechaIni).TotalHours;
                }
                if (servicio.CantidadBck.HasValue)
                {
                    tolHrasSolicitadas += subHrasSolicitadas * servicio.CantidadBck.Value;
                    tolHrasAprobadas += subHrasSolicitadas * (servicio.Cantidad == 0 ? 1 : servicio.Cantidad);
                }
                else
                    tolHrasSolicitadas += subHrasSolicitadas * (servicio.Cantidad == 0 ? 1 : servicio.Cantidad);
            }
            solicitud.HrasSolicitadas = tolHrasSolicitadas.ToString();
            solicitud.HrasAprobadas = tolHrasAprobadas.ToString();
            return solicitud;
        }
    }
}
