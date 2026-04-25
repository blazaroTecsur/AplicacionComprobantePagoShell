using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Resguardo.Application.Common;
using Resguardo.Application.Exceptions;
using Resguardo.Domain.Entities;
using Resguardo.Domain.Interfaces;

namespace Resguardo.Application.Commands.ConfirmarServicio
{
    public class ConfirmarServicioHandler
    {
        private readonly IUnidadTrabajo _unidadTrabajo;
        private readonly IValidator<ConfirmarServicioCommand> _fluentv;
        private readonly IMapper _mapeo;
        public ConfirmarServicioHandler(
            IValidator<ConfirmarServicioCommand> fluentv,
            IUnidadTrabajo unidadTrabajo,
            IMapper mapeo)
        {
            _fluentv = fluentv;
            _unidadTrabajo = unidadTrabajo;
            _mapeo = mapeo;
        }
        public async Task<bool> Ejecutar(ConfirmarServicioCommand formulario)
        {
            var validacion = await _fluentv.ValidateAsync(formulario);
            if (!validacion.IsValid)
                throw new ValidationException(validacion.Errors);

            var servicioProvs = _mapeo.Map<List<ServicioProv>>(formulario.Servicios);

            var solicitud = await _unidadTrabajo.SolicitudRepositorio.Obtener(formulario.IdSolicitud);
            if (solicitud is null)
                throw new BusinessException(StatusCodes.Status400BadRequest.ToString(),
                                            "No se pudo obtener los datos de la solicitud");
            var aprjefe = await _unidadTrabajo.GenericoRepositorio.Obtener(Constantes.TIP_ESTADO, Constantes.COD_ESTADO_APROBJEFE);
            if (aprjefe is null)
                throw new BusinessException(StatusCodes.Status400BadRequest.ToString(),
                                            "No se pudo obtener el estado");
            var aprcliente = await _unidadTrabajo.GenericoRepositorio.Obtener(Constantes.TIP_ESTADO, Constantes.COD_ESTADO_APROBCLIENT);
            if (aprcliente is null)
                throw new BusinessException(StatusCodes.Status400BadRequest.ToString(),
                                            "No se pudo obtener el estado");
            if (solicitud.IdEstado != aprjefe.Id)
                throw new BusinessException(StatusCodes.Status400BadRequest.ToString(),
                                            "El estado actual de la solicitud no es válido para esta operación.");

            solicitud.IdEstado = aprcliente.Id;
            await _unidadTrabajo.BeginTransactionAsync();
            try
            {
                await _unidadTrabajo.SolicitudRepositorio.Actualizar(solicitud);
                int cantCtta = 0;
                foreach (var servicio in solicitud.Servicios)
                {
                    cantCtta = servicioProvs.Where(x => x.IdServicio == servicio.Id).Sum(x => x.Cantidad);
                    if (cantCtta != servicio.Cantidad)
                    {

                        await _unidadTrabajo.RollbackTransactionAsync();
                        throw new BusinessException(StatusCodes.Status400BadRequest.ToString(),
                                                    "La cantidad imputada de efectivos por contratista difiere de la cantidad solicitada.");
                    }

                    foreach (var servicioProv in servicioProvs)
                    {
                        servicioProv.Estado = Constantes.COD_ESTADO_SERVPROV_CONFIRMADO;
                        if (servicioProv.Id == 0)
                            await _unidadTrabajo.ServicioProvRepositorio.Insertar(servicioProv);
                        else
                            await _unidadTrabajo.ServicioProvRepositorio.Actualizar(servicioProv);
                    }
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