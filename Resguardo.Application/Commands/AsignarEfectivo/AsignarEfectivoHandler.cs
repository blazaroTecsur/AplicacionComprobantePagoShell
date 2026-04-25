using FluentValidation;
using Microsoft.AspNetCore.Http;
using Resguardo.Application.Common;
using Resguardo.Application.Exceptions;
using Resguardo.Domain.Entities;
using Resguardo.Domain.Interfaces;

namespace Resguardo.Application.Commands.AsignarEfectivo
{
    public class AsignarEfectivoHandler
    {
        private readonly IUnidadTrabajo _unidadTrabajo;
        private readonly IValidator<AsignarEfectivoCommand> _fluentv;
        public AsignarEfectivoHandler(
            IValidator<AsignarEfectivoCommand> fluentv,
            IUnidadTrabajo unidadTrabajo)
        {
            _fluentv = fluentv;
            _unidadTrabajo = unidadTrabajo;
        }
        public async Task<bool> Ejecutar(AsignarEfectivoCommand formulario)
        {
            var validacion = await _fluentv.ValidateAsync(formulario);
            if (!validacion.IsValid)
                throw new ValidationException(validacion.Errors);

            var servicio = await _unidadTrabajo.ServicioProvRepositorio.Obtener(formulario.IdServicioProv);
            if (servicio.Cantidad != formulario.Efectivos.Count)
                throw new BusinessException(StatusCodes.Status400BadRequest.ToString(),
                                            "La cantidad de efectivos registrados no es coherente con lo solicitado.");
            servicio.Estado = Constantes.COD_ESTADO_SERVPROV_ASIGNADO;

            var fechaActual = DateTime.Now;
            var idPersona = 0;

            foreach (var efectivo in formulario.Efectivos)
            {
                var persona = await _unidadTrabajo.PersonalRepositorio.ObtenerPorDni(efectivo.Dni);
                if (persona is null)
                {
                    var personaIns = new Personal
                    {
                        Dni = efectivo.Dni,
                        Nombres = efectivo.Nombres,
                        Apellidos = efectivo.Apellidos,
                        Telefono = efectivo.Telefono,
                        UsuarioReg = "DBO",
                        FechaReg = fechaActual
                    };
                    await _unidadTrabajo.PersonalRepositorio.Insertar(personaIns);
                    await _unidadTrabajo.SaveChangesAsync();
                    if (personaIns.Id == 0)
                        throw new BusinessException(StatusCodes.Status400BadRequest.ToString(),
                                                    "No se registro correctamente los datos del efectivo");
                    idPersona = personaIns.Id;
                }
                else
                {
                    persona.Nombres = efectivo.Nombres;
                    persona.Apellidos = efectivo.Apellidos;
                    persona.Telefono = efectivo.Telefono;
                    persona.UsuarioAct = "DBO";
                    persona.FechaAct = fechaActual;
                    await _unidadTrabajo.PersonalRepositorio.Actualizar(persona);
                    idPersona = persona.Id;
                }

                servicio.Efectivos.Add(new Efectivo()
                {
                    IdServicioProv = formulario.IdServicioProv,
                    IdPersonal = idPersona,
                    Telefono = efectivo.Telefono,
                    UsuarioReg = "DBO",
                    FechaReg = fechaActual
                });
            }

            await _unidadTrabajo.ServicioProvRepositorio.Actualizar(servicio);
            await _unidadTrabajo.SaveChangesAsync();

            return true;
        }
    }
}
