using Microsoft.Extensions.Logging;
using Reciclaje.Aplicacion.DTOs.Reciclaje;
using Reciclaje.Aplicacion.Excepciones;
using Reciclaje.Aplicacion.Interfaces;
using Reciclaje.Dominio.Entidades;
using Reciclaje.Dominio.Interfaces;

namespace Reciclaje.Aplicacion.Servicios
{
    public class TareaOrdenCompraServicio : ITareaOrdenCompraServicio
    {
        private readonly ITareaOrdenCompraRepositorio _repositorio;
        private readonly ISyteLinePoServicio _syteLinePoServicio;
        private readonly ISyteLineServicio _syteLineServicio;
        private readonly ISyteLineTokenServicio _tokenServicio;
        private readonly ILogger<TareaOrdenCompraServicio> _logger;

        public TareaOrdenCompraServicio(
            ITareaOrdenCompraRepositorio repositorio,
            ISyteLinePoServicio syteLinePoServicio,
            ISyteLineServicio syteLineServicio,
            ISyteLineTokenServicio tokenServicio,
            ILogger<TareaOrdenCompraServicio> logger)
        {
            _repositorio = repositorio;
            _syteLinePoServicio = syteLinePoServicio;
            _syteLineServicio = syteLineServicio;
            _tokenServicio = tokenServicio;
            _logger = logger;
        }

        // ── Obtener todos ────────────────────────────────────────────
        public async Task<IEnumerable<TareaOrdenCompraDto>> ObtenerTodos()
        {
            var registros = await _repositorio.ObtenerTodos();

            return registros.Select(t => new TareaOrdenCompraDto
            {
                Id = t.Id,
                Anno = t.Anno,
                Mes = t.Mes,
                NombrePo = t.NombrePo,
                Sitio = t.Sitio,
                FechaCreacion = t.FechaCreacion,
                UsuarioCreacion = t.UsuarioCreacion,
                FechaModificacion = t.FechaModificacion,
                UsuarioModificacion = t.UsuarioModificacion,
                Estado = t.Estado,
                UidSyteLine = t.UidSyteLine
            });
        }

        // ── Obtener para editar ──────────────────────────────────────
        public async Task<TareaOrdenCompraEditarDto> ObtenerParaEditar(int id)
        {
            var tarea = await _repositorio.ObtenerPorId(id)
                ?? throw new NoEncontraExcepcion($"TareaOrdenCompra con ID {id} no encontrada.");

            return new TareaOrdenCompraEditarDto
            {
                Id = tarea.Id,
                Anno = tarea.Anno,
                Mes = tarea.Mes,
                NombrePo = tarea.NombrePo,
                Sitio = tarea.Sitio,
                Estado = tarea.Estado,
                UidSyteLine = tarea.UidSyteLine
            };
        }

        // ── Crear + enviar a SyteLine ────────────────────────────────
        public async Task<(bool exitoso, string mensaje)> Crear(
            TareaOrdenCompraCrearDto dto, string usuarioActual)
        {
            // Regla 1: único por anno, mes, sitio
            var existe = await _repositorio.ExistePorAnnoMesSitio(dto.Anno, dto.Mes, dto.Sitio);
            if (existe)
                throw new ValidacionExcepcion(
                    $"Ya existe una Orden de Compra para {dto.Anno}/{dto.Mes:D2}" +
                    (dto.Sitio is not null ? $" en el sitio '{dto.Sitio}'" : "") + ".");

            // Regla 2: NombrePO según sitio
            var nombrePo = GenerarNombrePo(dto.Anno, dto.Mes, dto.Sitio);

            // ── Paso 1: insertar en BD local ─────────────────────────
            var tarea = new Tareaordencompra
            {
                Anno = dto.Anno,
                Mes = dto.Mes,
                NombrePo = nombrePo,
                Sitio = dto.Sitio,
                FechaCreacion = DateTime.Now,
                UsuarioCreacion = usuarioActual,
                Estado = "Pendiente",
                UidSyteLine = dto.UidSyteLine
            };

            await _repositorio.Insertar(tarea);

            // ── Paso 2: obtener credenciales SyteLine ────────────────
            string accessToken;
            string mongooseConfig;
            try
            {
                var creds = await _tokenServicio.ObtenerCredencialesAsync();
                accessToken = creds.AccessToken;
                mongooseConfig = creds.MongooseConfig;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo obtener token SyteLine al crear TareaOrdenCompra {Id}.", tarea.Id);
                await _repositorio.Eliminar(tarea);
                return (false, $"Error al conectar con SyteLine para obtener token: {ex.Message}");
            }

            // ── Paso 3: crear PO en SyteLine ─────────────────────────
            var (exitoso, mensaje) = await _syteLinePoServicio.CrearOrdenCompraAsync(
                tareaId: tarea.Id,
                anno: tarea.Anno,
                mes: tarea.Mes,
                nombrePo: tarea.NombrePo,
                sitio: tarea.Sitio,
                accessToken: accessToken,
                mongooseConfig: mongooseConfig);

            // ── Paso 4: si fue exitoso, actualizar estado ─────────────
            if (exitoso)
            {
                tarea.Estado = "Activo";
                tarea.FechaModificacion = DateTime.Now;
                tarea.UsuarioModificacion = usuarioActual;
                await _repositorio.Actualizar(tarea);
            }

            return (exitoso, mensaje);
        }

        // ── Editar (sin integración SyteLine) ────────────────────────
        public async Task Editar(TareaOrdenCompraEditarDto dto, string usuarioActual)
        {
            var tarea = await _repositorio.ObtenerPorId(dto.Id)
                ?? throw new NoEncontraExcepcion($"TareaOrdenCompra con ID {dto.Id} no encontrada.");

            var existe = await _repositorio.ExistePorAnnoMesSitio(dto.Anno, dto.Mes, dto.Sitio, dto.Id);
            if (existe)
                throw new ValidacionExcepcion(
                    $"Ya existe una Orden de Compra para {dto.Anno}/{dto.Mes:D2}" +
                    (dto.Sitio is not null ? $" en el sitio '{dto.Sitio}'" : "") + ".");

            tarea.Anno = dto.Anno;
            tarea.Mes = dto.Mes;
            tarea.NombrePo = GenerarNombrePo(dto.Anno, dto.Mes, dto.Sitio);
            tarea.Sitio = dto.Sitio;
            tarea.Estado = dto.Estado;
            tarea.UidSyteLine = dto.UidSyteLine;
            tarea.FechaModificacion = DateTime.Now;
            tarea.UsuarioModificacion = usuarioActual;

            await _repositorio.Actualizar(tarea);
        }

        // ── Eliminar ─────────────────────────────────────────────────
        public async Task Eliminar(int id)
        {
            var tarea = await _repositorio.ObtenerPorId(id)
                ?? throw new NoEncontraExcepcion($"TareaOrdenCompra con ID {id} no encontrada.");

            await _repositorio.Eliminar(tarea);
        }

        // ── Helpers ──────────────────────────────────────────────────
        private static string GenerarNombrePo(short anno, byte mes, string? sitio = null)
        {
            var prefijo = sitio?.ToUpperInvariant() switch
            {
                "TECSUR" => "TECR",
                "GCI" => "REGCI",
                "LA" => "RELA",
                _ => "TECR"
            };
            return $"{prefijo}{mes:D2}{anno}";
        }
    }
}
