using Microsoft.AspNetCore.Http;
using Resguardo.Application.Common;
using Resguardo.Application.Common.Interfaces;
using Resguardo.Application.Exceptions;
using Resguardo.Domain.Interfaces;

namespace Resguardo.Application.Commands.AprobarAmplia
{
    public class AprobarAmpliaHandler
    {
        private readonly IUnidadTrabajo _unidadTrabajo;
        private readonly IUsuarioContexto _usuario;
        public AprobarAmpliaHandler(IUnidadTrabajo unidadTrabajo, IUsuarioContexto usuario)
        {
            _unidadTrabajo = unidadTrabajo;
            _usuario = usuario;
        }
        public async Task<bool> Ejecutar(AprobarAmpliaCommand formulario)
        {
            var efectivo = await _unidadTrabajo.EfectivoRepositorio.ObtenerPorId(formulario.IdEfectivo);
            if (efectivo is null)
                throw new NotFoundException("EFECTIVO");
            if (efectivo.EstAmplia != Constantes.AMPL_PENDIENTE)
                throw new BusinessException(
                        StatusCodes.Status400BadRequest.ToString(),
                        "El estado de la ampliación ya ha sido aprobado/rechazado anteriormente.");

            efectivo.EstAmplia = formulario.EstAmplia;
            efectivo.ComentApro = formulario.ComentApro;
            efectivo.UsuarioApro = _usuario.Correo;
            efectivo.FechaApro = DateTime.Now;
            await _unidadTrabajo.EfectivoRepositorio.Actualizar(efectivo);
            await _unidadTrabajo.SaveChangesAsync();
            return true;
        }
    }
}