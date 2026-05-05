using Microsoft.AspNetCore.Http;
using Resguardo.Application.Common;
using Resguardo.Application.Exceptions;
using Resguardo.Domain.Interfaces;

namespace Resguardo.Application.Commands.CancelarServicio
{
    public class CancelarServicioHandler
    {
        private readonly IUnidadTrabajo _unidadTrabajo;        
        public CancelarServicioHandler(IUnidadTrabajo unidadTrabajo)
        {           
            _unidadTrabajo = unidadTrabajo;
        }
        public async Task<bool> Ejecutar(CancelarServicioCommand formulario)
        {            
            var servicioProv = await _unidadTrabajo.ServicioProvRepositorio.ObtenerPorId(formulario.IdServicioProv);
            if (servicioProv is null)
                throw new BusinessException(StatusCodes.Status400BadRequest.ToString(), 
                    "No se pudo obtener los datos del servicio.");
            if (servicioProv.Estado == Constantes.SERVPROV_CERRADO)
                throw new BusinessException(StatusCodes.Status400BadRequest.ToString(),
                    "El estado actual del servicio no es válido para esta operación.");
            
            servicioProv.Estado = Constantes.SERVPROV_CANCELADO;            
            await _unidadTrabajo.ServicioProvRepositorio.Actualizar(servicioProv);
            await _unidadTrabajo.SaveChangesAsync();

            return true;
        }
    }
}
