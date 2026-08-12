using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Reciclaje.Aplicacion.DTOs.Reciclaje;
using Reciclaje.Aplicacion.Interfaces;
using Reciclaje.Dominio.Entidades;
using Reciclaje.Dominio.Interfaces;

namespace Reciclaje.Aplicacion.Servicios
{
    public class ValeConfirmacionServicio : IValeConfirmacionServicio
    {
        private readonly IValeRecuperoRepositorio _valeRepositorio;
        private readonly ISyteLinePoServicio _syteLinePoServicio;
        private readonly ISyteLineServicio _syteLineServicio;
        private readonly ITareaOrdenCompraRepositorio _tareaRepositorio;
        private readonly IConversionarticuloRepositorio<Conversionarticulo> _conversionRepositorio;
        private readonly ISyteLineTokenServicio _tokenServicio;
        private readonly ILogger<ValeConfirmacionServicio> _logger;
        private readonly IConfiguration _config;

        public ValeConfirmacionServicio(
            IValeRecuperoRepositorio valeRepositorio,
            ISyteLinePoServicio syteLinePoServicio,
            ISyteLineServicio syteLineServicio,
            ITareaOrdenCompraRepositorio tareaRepositorio,
            IConversionarticuloRepositorio<Conversionarticulo> conversionRepositorio,
            ISyteLineTokenServicio tokenServicio,
            ILogger<ValeConfirmacionServicio> logger,
            IConfiguration config)
        {
            _valeRepositorio = valeRepositorio;
            _syteLinePoServicio = syteLinePoServicio;
            _syteLineServicio = syteLineServicio;
            _tareaRepositorio = tareaRepositorio;
            _conversionRepositorio = conversionRepositorio;
            _tokenServicio = tokenServicio;
            _logger = logger;
            _config = config;
        }

        // ── 1. Buscar Vales Recepcionados / Confirmados ─────────────
        public async Task<ValeConfirmacionListaViewModel> BuscarValesConfirmacion(ValeRecuperoBuscarDto filtros)
        {
            var vales = await _valeRepositorio.Buscar(
                filtros.NumeroSro,
                filtros.NumeroVale,
                filtros.CodigoArticuloReciclaje,
                filtros.DescripcionArticuloReciclaje,
                filtros.FechaVale);

            var dtos = new List<ValeConfirmacionDto>();

            foreach (var v in vales.Where(v => v.Estado == "Recepcionado" || v.Estado == "Confirmado"))
            {
                var descripcion = string.Empty;
                if (!string.IsNullOrWhiteSpace(v.ArticuloReciclaje))
                {
                    var conv = await _conversionRepositorio
                        .ObtenerPorArticuloReciclaje(v.ArticuloReciclaje);
                    descripcion = conv?.DescripcionArticuloReciclaje ?? string.Empty;
                }

                dtos.Add(new ValeConfirmacionDto
                {
                    ValeId = v.ValeId,
                    NumeroVale = v.NumeroVale,
                    TipoVale = v.TipoVale,
                    NumeroSro = v.Srolinea.Sro.NumeroSro,
                    Contratista = v.Srolinea.Sro.DescripcionSubcontratista,
                    OrdenCompra = v.Srolinea.OrdenCompra,
                    UmReciclaje = v.Umreciclaje,
                    CantidadReciclaje = v.CantidadReciclaje,
                    OcAnual = v.Ocanual,
                    CodigoArticuloReciclaje = v.ArticuloReciclaje,
                    CantidadReal = v.CantidadRecibida,
                    Peso = v.PesoRecibido,
                    CantidadPendiente = (v.Srolinea.CantidadNoInv ?? 0) - (v.CantidadRecibida ?? 0),
                    CheckConfirmacion = v.CheckConfirmacion ?? false,
                    FechaConfirmacion = v.FechaConfirmacion,
                    Estado = v.Estado,
                    FechaCreacion = v.FechaCreacionAudit,
                    RowPointer = v.Srolinea.RowPointer,
                    ArticuloNoInventariado = v.Srolinea.ArticuloNoInv,
                    DescripcionArticuloReciclaje = descripcion,
                    FechaRecepcion = v.FechaRecepcion,
                    Ruc = v.Srolinea.Sro.Ruc
                });
            }

            return new ValeConfirmacionListaViewModel
            {
                Filtros = filtros,
                Vales = dtos,
                BusquedaRealizada = true
            };
        }

        // ── 2. Guardar confirmación + integración SyteLine ──────────
        public async Task GuardarConfirmacion(List<ValeConfirmacionGuardarDto> confirmaciones, string usuarioActual)
        {
            foreach (var conf in confirmaciones)
            {
                var vale = await _valeRepositorio.ObtenerPorId(conf.ValeId)
                    ?? throw new KeyNotFoundException($"ValeID {conf.ValeId} no encontrado.");

                if (vale.Estado != "Recepcionado") continue;

                if (!string.IsNullOrWhiteSpace(conf.ArticuloReciclaje))
                    vale.ArticuloReciclaje = conf.ArticuloReciclaje;

                vale.CantidadRecibida = conf.CantidadRecibida;
                vale.PesoRecibido = conf.PesoRecibido;
                vale.CheckConfirmacion = conf.CheckConfirmacion;
                vale.FechaConfirmacion = conf.CheckConfirmacion ? DateTime.Now : null;
                vale.Estado = conf.CheckConfirmacion ? "Confirmado" : "Recepcionado";
                vale.UsuarioModificacionAudit = usuarioActual;

                await _valeRepositorio.Actualizar(vale);
            }

            var valesConfirmados = confirmaciones.Where(c => c.CheckConfirmacion).ToList();
            if (!valesConfirmados.Any()) return;

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
                _logger.LogWarning(ex, "Token SyteLine no disponible; confirmación en BD guardada pero integración omitida.");
                return;
            }

            foreach (var conf in valesConfirmados)
            {
                var vale = await _valeRepositorio.ObtenerPorId(conf.ValeId);
                if (vale is null) continue;

                var poNum = vale.Ocanual;
                if (string.IsNullOrWhiteSpace(poNum)) continue;

                var tarea = await _tareaRepositorio.ObtenerPorNombrePo(poNum);
                if (tarea is null) continue;

                var (okLinea, _, ultimaLinea, rowPointer) =
                    await _syteLinePoServicio.ObtenerUltimaLineaPoAsync(poNum, accessToken, mongooseConfig);

                int lineaBase = (okLinea && ultimaLinea.HasValue) ? ultimaLinea.Value : (tarea.UltimaLinea ?? 0);

                if (okLinea && ultimaLinea.HasValue)
                {
                    tarea.UltimaLinea = ultimaLinea;
                    tarea.UidSyteLine = rowPointer ?? tarea.UidSyteLine;
                    tarea.FechaModificacion = DateTime.Now;
                    tarea.UsuarioModificacion = usuarioActual;
                    await _tareaRepositorio.Actualizar(tarea);
                }

                var codigoArticulo = vale.Srolinea?.ArticuloReciclaje ?? string.Empty;
                var descripcionArticulo = string.Empty;
                if (!string.IsNullOrWhiteSpace(codigoArticulo))
                {
                    var conversion = await _conversionRepositorio.ObtenerPorArticuloReciclaje(codigoArticulo);
                    descripcionArticulo = conversion?.DescripcionArticuloReciclaje ?? string.Empty;
                }

                var cantidad = conf.CantidadRecibida ?? vale.CantidadReciclaje ?? 0m;
                var unidadMedida = vale.Umreciclaje ?? "KG";

                var (okAdd, _, nuevaPoLine) = await _syteLinePoServicio.AgregarLineaPoAsync(
                    poNum, lineaBase, codigoArticulo, descripcionArticulo,
                    cantidad, unidadMedida, tarea.Anno, tarea.Mes, accessToken, mongooseConfig,
                    articuloNoInv: vale.Srolinea?.ArticuloReciclaje ?? string.Empty,
                    costoUnitario: vale.CostoUnitario ?? 0m);

                if (okAdd)
                {
                    tarea.UltimaLinea = nuevaPoLine;
                    tarea.FechaModificacion = DateTime.Now;
                    tarea.UsuarioModificacion = usuarioActual;
                    await _tareaRepositorio.Actualizar(tarea);
                }
            }
        }

        // ── 3. Rechazar vales ───────────────────────────────────────
        public async Task RechazarVales(List<ValeConfirmacionGuardarDto> confirmaciones, string usuarioActual)
        {
            foreach (var conf in confirmaciones.Where(c => c.Seleccionado))
            {
                var vale = await _valeRepositorio.ObtenerPorId(conf.ValeId)
                    ?? throw new KeyNotFoundException($"ValeID {conf.ValeId} no encontrado.");

                if (vale.Estado != "Recepcionado" && vale.Estado != "Confirmado") continue;

                vale.Estado = "Pendiente";
                vale.CheckRecepcion = false;
                vale.FechaRecepcion = null;
                vale.CheckConfirmacion = false;
                vale.FechaConfirmacion = null;
                vale.CantidadRecibida = null;
                vale.PesoRecibido = null;
                vale.UsuarioModificacionAudit = usuarioActual;

                await _valeRepositorio.Actualizar(vale);
            }
        }
    }
}
