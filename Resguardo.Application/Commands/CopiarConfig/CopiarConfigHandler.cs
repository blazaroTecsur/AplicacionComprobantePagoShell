using Microsoft.AspNetCore.Http;
using Seguridad.Abstractions.Interfaces;
using Resguardo.Application.Exceptions;
using Resguardo.Domain.Interfaces;

namespace Resguardo.Application.Commands.CopiarConfig
{
    public class CopiarConfigHandler
    {
        private readonly IUnidadTrabajo _unidadTrabajo;
        private readonly IUsuarioContexto _usuario;
        public CopiarConfigHandler(
            IUnidadTrabajo unidadTrabajo,
            IUsuarioContexto usuario)
        {
            _unidadTrabajo = unidadTrabajo;
            _usuario = usuario;
        }
        public async Task<bool> Ejecutar(CopiarConfigCommand formulario)
        {
            var configOrigen = await _unidadTrabajo.ConfigRepositorio.Listar(formulario.FechaOrigen);

            if (configOrigen is null || configOrigen.Count() == 0)
                throw new BusinessException(StatusCodes.Status400BadRequest.ToString(),
                        $"No existe una configuración con fecha de {formulario.FechaOrigen.ToString("dd/MM/yyyy")}");
            else if (DateOnly.FromDateTime(DateTime.Now.Date) > formulario.FechaDestino)
                throw new BusinessException(StatusCodes.Status400BadRequest.ToString(),
                        $"La fecha debe ser igual o mayor a la actual");
            else if (formulario.FechaOrigen == formulario.FechaDestino)
                throw new BusinessException(StatusCodes.Status400BadRequest.ToString(),
                        $"Las fechas no deben ser iguales");

            var configDestino = await _unidadTrabajo.ConfigRepositorio.Listar(formulario.FechaDestino);
            bool existeConfig = false;
            foreach (var config in configOrigen)
            {
                if (configDestino is not null)
                    existeConfig = configDestino.Where(x => x.IdTpoServicio == config.IdTpoServicio &&
                                                            x.CodDpto == config.CodDpto).Any();
                if (existeConfig)
                    continue;

                config.Id = 0;
                config.Fecha = formulario.FechaDestino;
                config.UsuarioReg = _usuario.Correo;
                config.FechaReg = DateTime.Now;
                await _unidadTrabajo.ConfigRepositorio.Insertar(config);
                existeConfig = false;
            }
            await _unidadTrabajo.SaveChangesAsync();

            return true;
        }
    }
}
