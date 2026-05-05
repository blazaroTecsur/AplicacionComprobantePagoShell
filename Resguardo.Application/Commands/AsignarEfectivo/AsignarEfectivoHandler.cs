using FluentValidation;
using Microsoft.AspNetCore.Http;
using Resguardo.Application.Common;
using Resguardo.Application.Common.Interfaces;
using Resguardo.Application.Exceptions;
using Resguardo.Domain.Entities;
using Resguardo.Domain.Interfaces;

namespace Resguardo.Application.Commands.AsignarEfectivo
{
    public class AsignarEfectivoHandler
    {
        private readonly IUnidadTrabajo _unidadTrabajo;
        private readonly IValidator<AsignarEfectivoCommand> _fluentv;
        private readonly IValidacionService _validacion;
        private readonly IUsuarioContexto _usuario;
        public AsignarEfectivoHandler(
            IValidator<AsignarEfectivoCommand> fluentv,
            IUnidadTrabajo unidadTrabajo,
            IValidacionService validacion,
            IUsuarioContexto usuario)
        {
            _fluentv = fluentv;
            _validacion = validacion;
            _unidadTrabajo = unidadTrabajo;
            _usuario = usuario;
        }
        public async Task<bool> Ejecutar(AsignarEfectivoCommand formulario)
        {
            var validacion = await _fluentv.ValidateAsync(formulario);
            if (!validacion.IsValid)
                throw new ValidationException(validacion.Errors);

            ValidationResult resultado = new(false, string.Empty);
            var servicio = await _unidadTrabajo.ServicioProvRepositorio.Obtener(formulario.IdServicioProv);
            if (servicio.Cantidad != formulario.Efectivos.Count)
                throw new BusinessException(StatusCodes.Status400BadRequest.ToString(),
                                            "La cantidad de efectivos registrados no es coherente con lo solicitado.");

            resultado = _validacion.ValidarAperturaAsignacion(servicio.ServicioNav.Fecha);
            if (!resultado.EsValido)
                throw new BusinessException(StatusCodes.Status400BadRequest.ToString(), resultado.Mensaje);

            var deTurno = await _unidadTrabajo.EfectivoRepositorio.ListarEfectivos(
                formulario.Efectivos.Select(e => e.Dni).ToArray(), servicio.ServicioNav.Fecha);
            if (deTurno is not null)
            {

                var fechaServicio = servicio.ServicioNav.Fecha;
                if (TimeSpan.TryParse(servicio.ServicioNav.HraFinal, out var horaServicio))
                {
                    var fechaIniServicio = fechaServicio.ToDateTime(TimeOnly.FromTimeSpan(horaServicio));
                    foreach (var efectivo in deTurno)
                    {
                        var fechaEfectivo = efectivo.ServicioProvNav.ServicioNav.Fecha.ToDateTime(TimeOnly.FromTimeSpan(TimeSpan.Parse(efectivo.ServicioProvNav.ServicioNav.HraFinal)));
                        _validacion.ValidarEfectivoEnTurno(
                            efectivo.ServicioProvNav.ServicioNav.DiaSig,
                            efectivo.PersonalNav.Dni,
                            fechaIniServicio,
                            fechaEfectivo);

                        if (!resultado.EsValido)
                            throw new BusinessException(StatusCodes.Status400BadRequest.ToString(), resultado.Mensaje);
                    }
                }
            }
            servicio.Estado = Constantes.SERVPROV_ASIGNADO;

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
                        UsuarioReg = _usuario.Correo,
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
                    persona.UsuarioAct = _usuario.Correo;
                    persona.FechaAct = fechaActual;
                    await _unidadTrabajo.PersonalRepositorio.Actualizar(persona);
                    idPersona = persona.Id;
                }

                servicio.Efectivos.Add(new Efectivo()
                {
                    IdServicioProv = formulario.IdServicioProv,
                    IdPersonal = idPersona,
                    Telefono = efectivo.Telefono,
                    UsuarioReg = _usuario.Correo,
                    FechaReg = fechaActual
                });
            }

            await _unidadTrabajo.ServicioProvRepositorio.Actualizar(servicio);
            await _unidadTrabajo.SaveChangesAsync();

            return true;
        }
    }
}