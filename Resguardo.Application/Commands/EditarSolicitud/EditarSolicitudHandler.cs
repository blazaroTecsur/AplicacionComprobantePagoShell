using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Notificacion.Abstractions;
using Notificacion.Domain.Models;
using Resguardo.Application.Common;
using Resguardo.Application.Common.Interfaces;
using Resguardo.Application.Exceptions;
using Resguardo.Domain.Entities;
using Resguardo.Domain.Interfaces;
using Seguridad.Abstractions.Interfaces;

namespace Resguardo.Application.Commands.EditarSolicitud
{
    public class EditarSolicitudHandler
    {
        private readonly IUnidadTrabajo _unidadTrabajo;
        private readonly IValidator<EditarSolicitudCommand> _fluentv;
        private readonly IValidacionService _validacion;
        private readonly IEmailQueue _email;
        private readonly ITemplateService _template;
        private readonly IConfiguration _config;
        private readonly IUsuarioContexto _usuario;
        public EditarSolicitudHandler(
            IValidator<EditarSolicitudCommand> fluentv,
            IValidacionService validacion,
            IUnidadTrabajo unidadTrabajo,
            IEmailQueue email,
            ITemplateService template,
            IConfiguration config,
            IUsuarioContexto usuario)
        {
            _fluentv = fluentv;
            _validacion = validacion;
            _unidadTrabajo = unidadTrabajo;
            _email = email;
            _template = template;
            _config = config;
            _usuario = usuario;
        }
        public async Task<bool> Ejecutar(EditarSolicitudCommand formulario)
        {
            var validacion = await _fluentv.ValidateAsync(formulario);
            if (!validacion.IsValid)
                throw new ValidationException(validacion.Errors);

            DateTime fecActual = DateTime.Now;
            ValidationResult resultado = new(false, string.Empty);

            var solicitud = await _unidadTrabajo.SolicitudRepositorio.Obtener(formulario.Id);
            var datosCambiados = CapturarCambios(solicitud, formulario);

            solicitud.CodCapataz = formulario.CodCapataz;
            solicitud.NomCapataz = formulario.NomCapataz;
            solicitud.Celular = formulario.Celular;
            solicitud.UsuarioAct = _usuario.Correo;
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

            if (!string.IsNullOrEmpty(datosCambiados))
                await EnviarCorreo(datosCambiados, solicitud.Folio, solicitud.UsuarioReg, solicitud.UsuarioApro);

            return true;
        }
        private string CapturarCambios(Solicitud anterior, EditarSolicitudCommand nuevo)
        {
            string datos = "";
            if (anterior.NomCapataz != nuevo.NomCapataz)
            {
                datos += "<tr>";
                datos += "<td>Campo: Capataz</td>";
                datos += $"<td>Valor anterior: {nuevo.NomCapataz}</td>";
                datos += $"<td>Valor nuevo: {nuevo.NomCapataz}</td>";
                datos += "</tr>";
            }
            if (anterior.Celular != nuevo.Celular)
            {
                datos += "<tr>";
                datos += "<td>Campo: Celular</td>";
                datos += $"<td>Valor anterior: {anterior.NomCapataz}</td>";
                datos += $"<td>Valor nuevo: {nuevo.NomCapataz}</td>";
                datos += "</tr>";
            }
            foreach (var servicio in anterior.Servicios)
            {
                var servicioForm = nuevo.Servicios.Where(s => s.Id == servicio.Id).FirstOrDefault();
                if (servicioForm is not null)
                {
                    if (servicio.HraInicio != servicioForm.HraInicio)
                    {
                        datos += "<tr>";
                        datos += "<td>Campo: Hra Inicio</td>";
                        datos += $"<td>Valor anterior: {servicio.HraInicio}</td>";
                        datos += $"<td>Valor nuevo: {servicioForm.HraInicio}</td>";
                        datos += "</tr>";
                    }
                    if (servicio.HraFinal != servicioForm.HraFinal)
                    {
                        datos += "<tr>";
                        datos += "<td>Campo: Hra Final</td>";
                        datos += $"<td>Valor anterior: {servicio.HraFinal}</td>";
                        datos += $"<td>Valor nuevo: {servicioForm.HraFinal}</td>";
                        datos += "</tr>";
                    }
                }
            }
            return datos;
        }
        private async Task EnviarCorreo(string datos, string folio, string registrador, string? aprobador)
        {
            string archivo = _config.GetSection("Templates:EditarSolicitud:Path").Value ?? string.Empty;
            string asunto = _config.GetSection("Templates:EditarSolicitud:Subject").Value ?? string.Empty;
            string html = await _template.GetTemplateAsync(archivo);

            html = html.Replace("${NombreUsuario}", _usuario.Titulo);
            html = html.Replace("${DatosModificados}", datos);
            html = html.Replace("${Folio}", folio);

            List<string> destinatarios = [];
            if (!string.IsNullOrEmpty(registrador))
                destinatarios.Add(registrador);
            if (!string.IsNullOrEmpty(aprobador))
                destinatarios.Add(aprobador);

            var email = new EmailMessage
            {
                To = new() { "dgutierrez@tecsur.com.pe", "lpalomino@tecsur.com.pe" },
                Cc = new() { _usuario.Correo },
                Subject = asunto,
                Body = html
            };

            _email.Enqueue(email);
        }
    }
}