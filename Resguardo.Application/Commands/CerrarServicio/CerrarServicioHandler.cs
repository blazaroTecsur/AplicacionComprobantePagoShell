using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Resguardo.Application.Common;
using Resguardo.Application.Exceptions;
using Resguardo.Domain.Interfaces;

namespace Resguardo.Application.Commands.CerrarServicio
{
    public class CerrarServicioHandler
    {
        private readonly IUnidadTrabajo _unidadTrabajo;
        private readonly IValidator<CerrarServicioCommand> _fluentv;        
        public CerrarServicioHandler(
            IValidator<CerrarServicioCommand> fluentv,
            IUnidadTrabajo unidadTrabajo)
        {
            _fluentv = fluentv;
            _unidadTrabajo = unidadTrabajo;            
        }
        public async Task<bool> Ejecutar(CerrarServicioCommand formulario)
        {
            var validacion = await _fluentv.ValidateAsync(formulario);
            if (!validacion.IsValid)
                throw new ValidationException(validacion.Errors);

            var servicioProv = await _unidadTrabajo.ServicioProvRepositorio.Obtener(formulario.IdServicioProv);
            if (servicioProv is null)
                throw new BusinessException(StatusCodes.Status400BadRequest.ToString(), "No se pudo obtener los datos del servicio.");
            servicioProv.Estado = Constantes.COD_ESTADO_SERVPROV_CERRADO;

            foreach (var efectivo in servicioProv.Efectivos)
            {
                var efectivoForm = formulario.Efectivos.Where(x => x.IdEfectivo == efectivo.Id).FirstOrDefault();
                if (efectivoForm is not null)
                {
                    efectivo.HraInicio = efectivoForm.HraInicio;
                    efectivo.HraFinal = efectivoForm.HraFinal;
                    efectivo.Asistio = efectivoForm.Asistio;
                    efectivo.SroRefencia = efectivoForm.SroRefencia;
                    efectivo.Comentario = efectivoForm.Comentario;
                    efectivo.UsuarioAct = "DBO";
                    efectivo.FechaAct = DateTime.Now;
                }
            }
            await _unidadTrabajo.ServicioProvRepositorio.Actualizar(servicioProv);
            await _unidadTrabajo.SaveChangesAsync();

            return true;
        }
    }
}
