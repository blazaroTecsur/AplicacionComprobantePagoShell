using AutoMapper;
using FluentValidation;
using Seguridad.Abstractions.Interfaces;
using Resguardo.Domain.Entities;
using Resguardo.Domain.Interfaces;

namespace Resguardo.Application.Commands.RegistrarConfig
{
    public class RegistrarConfigHandler
    {
        private readonly IUnidadTrabajo _unidadTrabajo;
        private readonly IValidator<RegistrarConfigCommand> _fluentv;
        private readonly IMapper _mapeo;
        private readonly IUsuarioContexto _usuario;
        public RegistrarConfigHandler(
            IValidator<RegistrarConfigCommand> fluentv,
            IUnidadTrabajo unidadTrabajo,
            IMapper mapeo,
            IUsuarioContexto usuario)
        {
            _fluentv = fluentv;
            _unidadTrabajo = unidadTrabajo;
            _mapeo = mapeo;
            _usuario = usuario;
        }
        public async Task<bool> Ejecutar(RegistrarConfigCommand formulario)
        {
            var validacion = await _fluentv.ValidateAsync(formulario);
            if (!validacion.IsValid)
                throw new ValidationException(validacion.Errors);

            var configs = _mapeo.Map<List<Limite>>(formulario.Configs);

            foreach (var config in configs)
            {
                config.Fecha = formulario.Fecha;
                config.UsuarioReg = _usuario.Correo;
                config.FechaReg = DateTime.Now;
                if (config.Id > 0)
                    await _unidadTrabajo.ConfigRepositorio.Actualizar(config);
                else
                    await _unidadTrabajo.ConfigRepositorio.Insertar(config);
            }
            await _unidadTrabajo.SaveChangesAsync();

            return true;
        }
    }
}