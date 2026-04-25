using Resguardo.Domain.Interfaces;

namespace Resguardo.Application.Commands.AprobarAmplia
{
    public class AprobarAmpliaHandler
    {
        private readonly IUnidadTrabajo _unidadTrabajo;
        public AprobarAmpliaHandler(IUnidadTrabajo unidadTrabajo)
        {
            _unidadTrabajo = unidadTrabajo;
        }
        public async Task<bool> Ejecutar(AprobarAmpliaCommand formulario)
        {
            var efectivo = await _unidadTrabajo.EfectivoRepositorio.ObtenerPorId(formulario.IdEfectivo);
            efectivo.EstAmplia = formulario.EstAmplia;
            efectivo.ComentApro = formulario.ComentApro;
            efectivo.UsuarioApro = "Luis Palomino";
            efectivo.FechaApro = DateTime.Now;
            await _unidadTrabajo.EfectivoRepositorio.Actualizar(efectivo);
            await _unidadTrabajo.SaveChangesAsync();
            return true;
        }
    }
}