using Microsoft.AspNetCore.Http;
using Resguardo.Application.Common;
using Resguardo.Application.Exceptions;
using Resguardo.Domain.Interfaces;

namespace Resguardo.Application.Commands.AmpliarServicio
{
    public class AmpliarServicioHandler
    {
        private readonly IUnidadTrabajo _unidadTrabajo;
        public AmpliarServicioHandler(IUnidadTrabajo unidadTrabajo)
        {
            _unidadTrabajo = unidadTrabajo;
        }
        public async Task<bool> Ejecutar(AmpliarServicioCommand formulario)
        {
            var servicioProv = await _unidadTrabajo.ServicioProvRepositorio.Obtener(formulario.IdServicioProv);
            var servicio = await _unidadTrabajo.ServicioRepositorio.ObtenerPorId(servicioProv.IdServicio);

            TimeSpan.TryParse(servicio.HraFinal, out var hraFinal);

            foreach (var efectivo in servicioProv.Efectivos)
            {
                var efectivoForm = formulario.Efectivos.Where(x => x.IdEfectivo == efectivo.Id).FirstOrDefault();
                if (efectivoForm is not null)
                {
                    if (string.IsNullOrEmpty(efectivoForm.HraAmplia))
                        throw new BusinessException(
                            StatusCodes.Status400BadRequest.ToString(),
                            "Debe ingresar la hora de ampliación");
                    if (!TimeSpan.TryParse(efectivoForm.HraAmplia, out var hraAmplia))
                        throw new BusinessException(
                            StatusCodes.Status400BadRequest.ToString(),
                            "La hora de ampliación es inválida");
                    if (hraFinal >= hraAmplia)
                        throw new BusinessException(
                                StatusCodes.Status400BadRequest.ToString(),
                                "La hora de ampliación no puede ser menor o igual a la hora final del servicio");

                    efectivo.HraAmplia = efectivoForm.HraAmplia;
                    efectivo.EstAmplia = Constantes.AMPL_PENDIENTE;
                }
            }
            await _unidadTrabajo.ServicioProvRepositorio.Actualizar(servicioProv);
            await _unidadTrabajo.SaveChangesAsync();
            return true;
        }
    }
}
