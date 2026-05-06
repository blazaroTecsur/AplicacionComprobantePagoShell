using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Resguardo.Application.Common;
using Resguardo.Application.Common.Interfaces;
using Resguardo.Application.Exceptions;
using Resguardo.Domain.Entities;
using Resguardo.Domain.Interfaces;
using Seguridad.Abstractions.Interfaces;

namespace Resguardo.Application.Commands.ActualizarSolicitud
{
    public class ActualizarSolicitudHandler
    {
        private readonly IUnidadTrabajo _unidadTrabajo;
        private readonly IValidator<ActualizarSolicitudCommand> _fluentv;
        private readonly IValidacionService _validacion;
        private readonly IMapper _mapeo;
        private readonly IUsuarioContexto _usuario;
        public ActualizarSolicitudHandler(
            IValidator<ActualizarSolicitudCommand> fluentv,
            IValidacionService validacion,
            IUnidadTrabajo unidadTrabajo,
            IMapper mapeo,
            IUsuarioContexto usuario)
        {
            _fluentv = fluentv;
            _validacion = validacion;
            _unidadTrabajo = unidadTrabajo;
            _mapeo = mapeo;
            _usuario = usuario;
        }
        public async Task<int> Ejecutar(ActualizarSolicitudCommand formulario)
        {
            var validacion = await _fluentv.ValidateAsync(formulario);
            if (!validacion.IsValid)
                throw new ValidationException(validacion.Errors);

            DateTime fecActual = DateTime.Now;
            ValidationResult resultado = new(false, string.Empty);

            var solicitud = await _unidadTrabajo.SolicitudRepositorio.Obtener(formulario.Id);
            solicitud.CodCapataz = formulario.CodCapataz;
            solicitud.NomCapataz = formulario.NomCapataz;
            solicitud.Celular = formulario.Celular;
            solicitud.TpoTrabajo = formulario.TpoTrabajo;
            solicitud.UsuarioAct = _usuario.Correo;
            solicitud.FechaAct = fecActual;

            var servicios = _mapeo.Map<List<Servicio>>(formulario.Servicios);

            if (solicitud.EstadoNav.Codigo != Constantes.COD_ESTADO_INGRESADO)
                throw new BusinessException(StatusCodes.Status400BadRequest.ToString(),
                    "El estado actual de la solicitud no es válido para esta operación.");
            if (servicios.Count > Constantes.CANT_MAX_X_SERV)
                throw new BusinessException(StatusCodes.Status400BadRequest.ToString(),
                    $"No puede agregar más de {Constantes.CANT_MAX_X_SERV} servicios a la solicitud.");

            int contador = solicitud.Servicios.Count + 1;
            foreach (var servicio in servicios)
            {
                if (servicio.Id == 0)
                {
                    servicio.IdSolicitud = solicitud.Id;
                    servicio.UsuarioReg = _usuario.Correo;
                    servicio.FechaReg = fecActual;

                    if (servicio.Fecha < DateOnly.FromDateTime(fecActual.Date))
                        throw new BusinessException(
                            StatusCodes.Status400BadRequest.ToString(),
                            "La fecha del servicio no puede ser menor a la fecha actual.");
                    if (servicio.Fecha != servicios.Select(x => x.Fecha).First())
                        throw new BusinessException(
                                StatusCodes.Status400BadRequest.ToString(),
                                "Los servicios deben de solicitarse para una única fecha.");
                    if (!TimeSpan.TryParse(servicio.HraInicio, out var horInicio))
                        throw new BusinessException(
                            StatusCodes.Status400BadRequest.ToString(),
                            "La hora de inicio es inválida");
                    if (!TimeSpan.TryParse(servicio.HraFinal, out var horFinal))
                        throw new BusinessException(
                            StatusCodes.Status400BadRequest.ToString(),
                            "La hora final es inválida");

                    resultado = _validacion.ValidarRangoHorario(
                        servicio.DiaSig,
                        servicio.Fecha.ToDateTime(TimeOnly.MinValue),
                        horInicio,
                        horFinal);
                    if (!resultado.EsValido)
                        throw new BusinessException(StatusCodes.Status400BadRequest.ToString(), resultado.Mensaje);

                    if (solicitud.FlujoNav.Codigo == Constantes.COD_FLUJO_REGULAR)
                    {
                        resultado = _validacion.ValidarServicioRegular(fecActual, servicio.Fecha, TimeOnly.FromTimeSpan(horInicio));
                        if (!resultado.EsValido)
                            throw new BusinessException(StatusCodes.Status400BadRequest.ToString(), resultado.Mensaje);
                    }
                    else if (solicitud.FlujoNav.Codigo == Constantes.COD_FLUJO_URGENCIA)
                    {
                        resultado = _validacion.ValidarServicioUrgencia(fecActual, servicio.Fecha, horInicio);
                        if (!resultado.EsValido)
                            throw new BusinessException(StatusCodes.Status400BadRequest.ToString(), resultado.Mensaje);
                    }
                    servicio.Turno = _validacion.ObtenerTurno(horInicio);
                    servicio.Folio = $"{solicitud.Folio}-{contador.ToString().PadLeft(2, '0')}";
                    solicitud.Servicios.Add(servicio);
                    contador++;
                }
            }

            var servicioEliminar = solicitud.Servicios
                                .Where(x => !servicios.Any(f => f.Id == x.Id))
                                .ToList();
            foreach (var servicio in servicioEliminar)
                await _unidadTrabajo.ServicioRepositorio.Eliminar(servicio.Id);
            await _unidadTrabajo.SolicitudRepositorio.Actualizar(solicitud);
            await _unidadTrabajo.SaveChangesAsync();

            return solicitud.Id;
        }
    }
}