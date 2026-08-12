using Reciclaje.Aplicacion.DTOs.Reciclaje;
using Reciclaje.Aplicacion.Interfaces;
using Reciclaje.Dominio.Entidades;
using Reciclaje.Dominio.Interfaces;

namespace Reciclaje.Aplicacion.Servicios
{
    public class ConversionarticuloServicio : IConversionarticuloServicio
    {
        private readonly IConversionarticuloRepositorio<Conversionarticulo> _repositorio;

        public ConversionarticuloServicio(
            IConversionarticuloRepositorio<Conversionarticulo> repositorio)
        {
            _repositorio = repositorio;
        }

        public async Task<IEnumerable<ConversionarticuloDto>> Listar()
        {
            var lista = await _repositorio.Listar();
            return lista.Select(MapearADto);
        }

        public async Task<ConversionarticuloDto> ObtenerPorId(int id)
        {
            var entidad = await _repositorio.ObtenerPorId(id);
            return MapearADto(entidad);
        }

        public async Task Crear(ConversionarticuloCrearDto dto, string usuarioActual)
        {
            var entidad = new Conversionarticulo
            {
                ArticuloNoInventariado = dto.ArticuloNoInventariado,
                ArticuloReciclaje = dto.ArticuloReciclaje,
                DescripcionArticuloReciclaje = dto.DescripcionArticuloReciclaje,
                UsuarioCreacionAudit = usuarioActual
            };
            await _repositorio.Insertar(entidad);
        }

        public async Task Editar(ConversionarticuloEditarDto dto, string usuarioActual)
        {
            var entidad = await _repositorio.ObtenerPorId(dto.ConversionId);

            entidad.ArticuloNoInventariado = dto.ArticuloNoInventariado;
            entidad.ArticuloReciclaje = dto.ArticuloReciclaje;
            entidad.DescripcionArticuloReciclaje = dto.DescripcionArticuloReciclaje;
            entidad.UsuarioModificacionAudit = usuarioActual;

            _repositorio.Actualizar(entidad);
        }

        public async Task Eliminar(int id) =>
            await _repositorio.Eliminar(id);

        // ── Mapper privado (sin AutoMapper para mantener dependencias mínimas) ──
        private static ConversionarticuloDto MapearADto(Conversionarticulo e) => new()
        {
            ConversionId = e.ConversionId,
            ArticuloNoInventariado = e.ArticuloNoInventariado,
            ArticuloReciclaje = e.ArticuloReciclaje,
            DescripcionArticuloReciclaje = e.DescripcionArticuloReciclaje,
            FechaCreacionAudit = e.FechaCreacionAudit,
            UsuarioCreacionAudit = e.UsuarioCreacionAudit,
            FechaModificacionAudit = e.FechaModificacionAudit,
            UsuarioModificacionAudit = e.UsuarioModificacionAudit
        };
    }
}