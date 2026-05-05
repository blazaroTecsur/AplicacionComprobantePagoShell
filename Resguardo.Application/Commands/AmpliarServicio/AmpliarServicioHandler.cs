using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Notificacion.Abstractions;
using Notificacion.Domain.Models;
using Resguardo.Application.Common;
using Resguardo.Application.Common.Interfaces;
using Resguardo.Application.Exceptions;
using Resguardo.Application.Interfaces.Background;
using Resguardo.Domain.Interfaces;

namespace Resguardo.Application.Commands.AmpliarServicio
{
    public class AmpliarServicioHandler
    {
        private readonly IUnidadTrabajo _unidadTrabajo;
        private readonly IValidacionService _validacion;
        private readonly IEmailQueue _email;
        private readonly ITemplateService _template;
        private readonly IConfiguration _config;
        private readonly IUsuarioContexto _usuario;
        public AmpliarServicioHandler(
            IUnidadTrabajo unidadTrabajo,
            IValidacionService validacion,
            IEmailQueue email,
            ITemplateService template,
            IConfiguration config,
            IUsuarioContexto usuario)
        {
            _unidadTrabajo = unidadTrabajo;
            _validacion = validacion;
            _email = email;
            _template = template;
            _config = config;
            _usuario = usuario;
        }
        public async Task<bool> Ejecutar(AmpliarServicioCommand formulario)
        {
            var efectivo = await _unidadTrabajo.EfectivoRepositorio.Obtener(formulario.IdEfectivo);
            if (efectivo is null)
                throw new NotFoundException("EFECTIVO");
            if (efectivo.EstAmplia == Constantes.AMPL_RECHAZADO || efectivo.EstAmplia == Constantes.AMPL_APROBADO)
                throw new BusinessException(
                        StatusCodes.Status400BadRequest.ToString(),
                        "El estado de la ampliación ya ha sido aprobado/rechazado anteriormente.");

            TimeSpan.TryParse(efectivo.ServicioProvNav.ServicioNav.HraFinal, out var hraFinal);

            var fechaFinServicio = efectivo.ServicioProvNav.ServicioNav.Fecha.ToDateTime(TimeOnly.MinValue).Add(hraFinal);
            ValidationResult resultado = _validacion.ValidarHoraAmpliar(fechaFinServicio);

            if (!resultado.EsValido)
                throw new BusinessException(
                        StatusCodes.Status400BadRequest.ToString(),
                        resultado.Mensaje);
            if (string.IsNullOrEmpty(formulario.HraAmplia))
                throw new BusinessException(
                    StatusCodes.Status400BadRequest.ToString(),
                    "Debe ingresar la hora de ampliación");
            if (!TimeSpan.TryParse(formulario.HraAmplia, out var hraAmplia))
                throw new BusinessException(
                    StatusCodes.Status400BadRequest.ToString(),
                    "La hora de ampliación es inválida");
            if (hraFinal >= hraAmplia)
                throw new BusinessException(
                        StatusCodes.Status400BadRequest.ToString(),
                        "La hora de ampliación no puede ser menor o igual a la hora final del servicio");

            efectivo.HraAmplia = formulario.HraAmplia;
            efectivo.EstAmplia = Constantes.AMPL_PENDIENTE;

            await _unidadTrabajo.EfectivoRepositorio.Actualizar(efectivo);
            await _unidadTrabajo.SaveChangesAsync();
            await EnviarCorreo(
                efectivo.ServicioProvNav.ServicioNav.HraFinal,
                formulario.HraAmplia,
                efectivo.ServicioProvNav.ServicioNav.Folio);

            return true;
        }
        private async Task EnviarCorreo(string hraFinal, string hraAmplia, string folio)
        {
            string archivo = _config.GetSection("Templates:AmpliarServicio:Path").Value ?? string.Empty;
            string asunto = _config.GetSection("Templates:AmpliarServicio:Subject").Value ?? string.Empty;
            string html = await _template.GetTemplateAsync(archivo);

            html = html.Replace("${NombreUsuario}", _usuario.Titulo);
            html = html.Replace("${HraFinal}", hraFinal);
            html = html.Replace("${HraAmplia}", hraAmplia);
            html = html.Replace("${Folio}", folio);

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
