using FluentValidation;
using Microsoft.AspNetCore.Http;
using Resguardo.Application.Common;
using Resguardo.Application.Exceptions;
using Resguardo.Domain.Interfaces;

namespace Resguardo.Application.Commands.AprobarSolicitud
{
    public class AprobarSolicitudHandler
    {
        private readonly IUnidadTrabajo _unidadTrabajo;
        private readonly IValidator<AprobarSolicitudCommand> _fluentv;
        public AprobarSolicitudHandler(
            IValidator<AprobarSolicitudCommand> fluentv,
            IUnidadTrabajo unidadTrabajo)
        {
            _unidadTrabajo = unidadTrabajo;
            _fluentv = fluentv;
        }
        public async Task<bool> Ejecutar(AprobarSolicitudCommand formulario)
        {
            var validacion = await _fluentv.ValidateAsync(formulario);
            if (!validacion.IsValid)
                throw new ValidationException(validacion.Errors);

            var solicitud = await _unidadTrabajo.SolicitudRepositorio.Obtener(formulario.Id);
            if (solicitud is null)
                throw new BusinessException(StatusCodes.Status400BadRequest.ToString(),
                                            "No se pudo obtener los datos de la solicitud");
            var estadoNuevo = await _unidadTrabajo.GenericoRepositorio.Obtener(Constantes.TIP_ESTADO, formulario.CodEstado);
            if (estadoNuevo is null)
                throw new BusinessException(StatusCodes.Status400BadRequest.ToString(),
                                            "No se pudo obtener el estado");            
            if (solicitud.EstadoNav.Codigo != Constantes.COD_ESTADO_INGRESADO)
                throw new BusinessException(StatusCodes.Status400BadRequest.ToString(),
                                            "El estado actual de la solicitud no es válido para esta operación.");

            solicitud.IdEstado = estadoNuevo.Id;
            solicitud.Comentario = formulario.Comentario;
            solicitud.UsuarioApro = "Fredy Crispin";
            solicitud.FechaApro = DateTime.Now;

            if (formulario.CodEstado == Constantes.COD_ESTADO_APROBJEFE)
            {
                foreach (var servicio in solicitud.Servicios)
                {
                    var servicioForm = formulario.Servicios.FirstOrDefault(s => s.Id == servicio.Id);
                    if (servicioForm is not null)
                    {
                        servicio.CantidadBck = servicio.Cantidad;
                        servicio.Cantidad = servicioForm.Cantidad;
                        servicio.Comentario = servicioForm.Comentario;
                    }
                }
            }
            await _unidadTrabajo.BeginTransactionAsync();
            try
            {
                await _unidadTrabajo.SolicitudRepositorio.Actualizar(solicitud);
                await _unidadTrabajo.SaveChangesAsync();

                var servicioAgp = solicitud.Servicios
                    .GroupBy(x => new { x.IdTpoServicio, x.Fecha, x.Turno })
                    .Select(x => new { x.Key.IdTpoServicio, x.Key.Fecha, x.Key.Turno });

                int cantSolicitada = 0, cantPermitida = 0;
                foreach (var servicio in servicioAgp)
                {
                    cantSolicitada = await _unidadTrabajo.ServicioRepositorio
                       .CalularCantidad(solicitud.CodDpto, servicio.IdTpoServicio, servicio.Fecha, servicio.Turno);

                    if (servicio.Turno == Constantes.TURNO_DIURNO)
                    {
                        cantPermitida = await _unidadTrabajo.ConfigRepositorio
                           .ObtenerDiurno(solicitud.CodDpto, servicio.IdTpoServicio, servicio.Fecha);
                    }
                    else if (servicio.Turno == Constantes.TURNO_NOCTURNO)
                    {
                        cantPermitida = await _unidadTrabajo.ConfigRepositorio
                           .ObtenerNocturno(solicitud.CodDpto, servicio.IdTpoServicio, servicio.Fecha);
                    }
                    if (cantSolicitada > cantPermitida)
                        throw new BusinessException(StatusCodes.Status400BadRequest.ToString(),
                                                "La cantidad solicitud excede de la cantidad permitida. Coordine con el área de SAD.");

                }
                await _unidadTrabajo.CommitTransactionAsync();
            }
            catch
            {
                await _unidadTrabajo.RollbackTransactionAsync();
                throw;
            }

            return true;
        }
    }
}