using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Resguardo.Application.Common;
using Resguardo.Application.Common.Interfaces;
using Resguardo.Application.Exceptions;
using Resguardo.Application.Services;
using Resguardo.Domain.Entities;
using Resguardo.Domain.Interfaces;

namespace Resguardo.Application.Commands.RegistrarSolicitud
{
    public class RegistrarSolicitudHandler
    {
        private readonly IUnidadTrabajo _unidadTrabajo;
        private readonly IInforService _syteline;
        private readonly IValidacionService _validacion;
        private readonly IMapper _mapeo;
        private readonly IValidator<RegistrarSolicitudCommand> _fluentv;
        private readonly IUsuarioContexto _usuario;
        public RegistrarSolicitudHandler(
            IValidator<RegistrarSolicitudCommand> fluentv,
            IUnidadTrabajo unidadTrabajo,
            IInforService syteline,
            IValidacionService validacion,
            IMapper mapeo,
            IUsuarioContexto usuario)
        {
            _fluentv = fluentv;
            _unidadTrabajo = unidadTrabajo;
            _validacion = validacion;
            _syteline = syteline;
            _mapeo = mapeo;
            _usuario = usuario;
        }
        public async Task<RegistrarSolicitudResponse> Ejecutar(RegistrarSolicitudCommand formulario)
        {
            var validacion = await _fluentv.ValidateAsync(formulario);
            if (!validacion.IsValid)
                throw new ValidationException(validacion.Errors);

            var solicitud = _mapeo.Map<Solicitud>(formulario);

            if (solicitud.Servicios.Count > Constantes.CANT_MAX_X_SERV)
                throw new BusinessException(StatusCodes.Status400BadRequest.ToString(),
                    $"No puede agregar más de {Constantes.CANT_MAX_X_SERV} servicios a la solicitud.");
            var folio = await _unidadTrabajo.GenericoRepositorio.Obtener(Constantes.TIP_FOLIO, Constantes.COD_FOLIO);
            if (folio is null)
                throw new BusinessException(StatusCodes.Status400BadRequest.ToString(),
                    "No se pudo generar el folio");
            var estado = await _unidadTrabajo.GenericoRepositorio.Obtener(Constantes.TIP_ESTADO, Constantes.COD_ESTADO_INGRESADO);
            if (estado is null)
                throw new BusinessException(StatusCodes.Status400BadRequest.ToString(),
                    "No se pudo obtener el estado");
            var tipo = await _unidadTrabajo.GenericoRepositorio.Obtener(Constantes.TIP_TIPO, Constantes.COD_TIPO_SOLICITUD);
            if (tipo is null)
                throw new BusinessException(StatusCodes.Status400BadRequest.ToString(),
                    "No se pudo obtener el tipo");
            var flujo = await _unidadTrabajo.GenericoRepositorio.ObtenerPorId(solicitud.IdFlujo);
            if (flujo is null)
                throw new BusinessException(StatusCodes.Status400BadRequest.ToString(),
                    "No se pudo obtener el flujo");
            var orden = await _syteline.ObtenerOrden(solicitud.NumSro);
            if (orden is null)
                throw new BusinessException(StatusCodes.Status400BadRequest.ToString(),
                    "No se pudo obtener el flujo");

            DateTime fecActual = DateTime.Now;
            ValidationResult resultado = new(false, string.Empty);

            solicitud.CodDpto = orden.CodDpto;
            solicitud.NomDpto = orden.NomDpto;
            solicitud.CodActv = orden.CodActv;
            solicitud.NomActv = orden.NomActv;
            solicitud.CodSupr = orden.CodSupr;
            solicitud.NomSupr = orden.NomSupr;
            solicitud.RucSctta = orden.RucSctta;
            solicitud.NomSctta = orden.NomSctta;
            solicitud.FechaFoc = orden.FechaFoc;
            solicitud.Coordenada = orden.Coordenada;
            solicitud.Direccion = orden.Direccion;
            solicitud.Folio = FolioSecuencial(folio.Descripcion);
            solicitud.IdEstado = estado.Id;
            solicitud.IdTipo = tipo.Id;
            solicitud.UsuarioReg = _usuario.Correo;
            solicitud.FechaReg = fecActual;

            int contador = 1;
            foreach (var servicio in solicitud.Servicios)
            {
                servicio.IdSolicitud = solicitud.Id;
                servicio.UsuarioReg = _usuario.Correo;
                servicio.FechaReg = fecActual;

                if (servicio.Fecha < DateOnly.FromDateTime(fecActual.Date))
                    throw new BusinessException(
                        StatusCodes.Status400BadRequest.ToString(),
                        "La fecha del servicio no puede ser menor a la fecha actual.");
                if (servicio.Fecha != solicitud.Servicios.Select(x => x.Fecha).First())
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

                resultado = _validacion.ValidarRangoHorario(servicio.DiaSig, servicio.Fecha.ToDateTime(TimeOnly.MinValue), horInicio, horFinal);
                if (!resultado.EsValido)
                    throw new BusinessException(StatusCodes.Status400BadRequest.ToString(), resultado.Mensaje);

                if (flujo.Codigo == Constantes.COD_FLUJO_REGULAR)
                {
                    resultado = _validacion.ValidarServicioRegular(fecActual, servicio.Fecha, TimeOnly.FromTimeSpan(horInicio));
                    if (!resultado.EsValido)
                        throw new BusinessException(StatusCodes.Status400BadRequest.ToString(), resultado.Mensaje);
                }
                else if (flujo.Codigo == Constantes.COD_FLUJO_URGENCIA)
                {
                    resultado = _validacion.ValidarServicioUrgencia(fecActual, servicio.Fecha, horInicio);
                    if (!resultado.EsValido)
                        throw new BusinessException(StatusCodes.Status400BadRequest.ToString(), resultado.Mensaje);
                }
                servicio.Turno = _validacion.ObtenerTurno(horInicio);
                servicio.Folio = $"{solicitud.Folio}-{contador.ToString().PadLeft(2, '0')}";
                contador++;
            }

            var generico = await _unidadTrabajo.GenericoRepositorio.ObtenerPorId(folio.Id);
            generico.Descripcion = Convert.ToInt32(solicitud.Folio).ToString();
            generico.UsuarioAct = _usuario.Correo;
            generico.FechaAct = fecActual;

            await _unidadTrabajo.BeginTransactionAsync();
            try
            {
                await _unidadTrabajo.GenericoRepositorio.Actualizar(generico);
                await _unidadTrabajo.SolicitudRepositorio.Insertar(solicitud);
                await _unidadTrabajo.CommitTransactionAsync();
            }
            catch
            {
                await _unidadTrabajo.RollbackTransactionAsync();
                throw;
            }
            return new RegistrarSolicitudResponse { Id = solicitud.Id, Folio = solicitud.Folio };
        }
        private static string FolioSecuencial(string folio)
        {
            string secuencial = (Convert.ToInt32(folio) + 1).ToString().PadLeft(6, '0');
            return secuencial;
        }
    }
}