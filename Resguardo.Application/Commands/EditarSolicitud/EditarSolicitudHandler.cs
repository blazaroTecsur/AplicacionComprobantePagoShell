using FluentValidation;
using Microsoft.AspNetCore.Http;
using Resguardo.Application.Common;
using Resguardo.Application.Common.Interfaces;
using Resguardo.Application.Exceptions;
using Resguardo.Domain.Interfaces;

namespace Resguardo.Application.Commands.EditarSolicitud
{
    public class EditarSolicitudHandler
    {
        private readonly IUnidadTrabajo _unidadTrabajo;
        private readonly IValidator<EditarSolicitudCommand> _fluentv;
        private readonly IValidacionService _validacion;        
        public EditarSolicitudHandler(
            IValidator<EditarSolicitudCommand> fluentv,
            IValidacionService validacion,
            IUnidadTrabajo unidadTrabajo)
        {
            _fluentv = fluentv;
            _validacion = validacion;
            _unidadTrabajo = unidadTrabajo;
        }
        public async Task<bool> Ejecutar(EditarSolicitudCommand formulario)
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
            solicitud.UsuarioAct = "DBO";
            solicitud.FechaAct = fecActual;

            var fechaServicio = solicitud.Servicios.Select(x => x.Fecha).First();
            var fechaLimite = fechaServicio
                .AddDays(-1)
                .ToDateTime(new TimeOnly(Constantes.HRA_LIMIT_EDIT, 0));

            if (fecActual > fechaLimite)
                throw new BusinessException(
                                StatusCodes.Status400BadRequest.ToString(),
                                "Ya no se puede editar la solicitud (se encuentra fuera del horario permitido).");

            foreach (var servicio in solicitud.Servicios)
            {
                var servicioForm = formulario.Servicios.FirstOrDefault(s => s.Id == servicio.Id);
                if (servicioForm is not null)
                {

                    if (!TimeSpan.TryParse(servicioForm.HraInicio, out var horInicio))
                        throw new BusinessException(
                            StatusCodes.Status400BadRequest.ToString(),
                            "La hora de inicio es inválida");
                    if (!TimeSpan.TryParse(servicioForm.HraFinal, out var horFinal))
                        throw new BusinessException(
                            StatusCodes.Status400BadRequest.ToString(),
                            "La hora final es inválida");

                    TimeSpan.TryParse(servicio.HraInicio, out var horInicioAct);
                    TimeSpan.TryParse(servicio.HraFinal, out var horFinalAct);

                    resultado = _validacion.ValidarHorarioFijado(
                        servicio.DiaSig,
                        servicio.Fecha.ToDateTime(TimeOnly.MinValue),
                        horInicioAct,
                        horFinalAct,
                        horInicio,
                        horFinal);
                    if (!resultado.EsValido)
                        throw new BusinessException(StatusCodes.Status400BadRequest.ToString(), resultado.Mensaje);

                    servicio.HraInicio = servicioForm.HraInicio;
                    servicio.HraFinal = servicioForm.HraFinal;
                    servicio.Turno = _validacion.ObtenerTurno(horInicio);
                }
            }
            await _unidadTrabajo.SolicitudRepositorio.Actualizar(solicitud);
            await _unidadTrabajo.SaveChangesAsync();

            return true;
        }
    }
}