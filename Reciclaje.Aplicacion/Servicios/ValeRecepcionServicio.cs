using Reciclaje.Aplicacion.DTOs.Reciclaje;
using Reciclaje.Aplicacion.Interfaces;
using Reciclaje.Dominio.Interfaces;

namespace Reciclaje.Aplicacion.Servicios
{
    /// <summary>
    /// Implementa la lógica de búsqueda y registro de recepción
    /// de vales (vista RecepcionarVale).
    /// </summary>
    public class ValeRecepcionServicio : IValeRecepcionServicio
    {
        private readonly ISroRepositorio _sroRepositorio;
        private readonly IValeRecuperoRepositorio _valeRepositorio;
        private readonly IValeRecuperoDetalleRepositorio _detalleRepositorio;
        private readonly ISyteLineServicio _syteLineServicio;
        private readonly ISyteLineTokenServicio _tokenServicio;

        public ValeRecepcionServicio(
            ISroRepositorio sroRepositorio,
            IValeRecuperoRepositorio valeRepositorio,
            IValeRecuperoDetalleRepositorio detalleRepositorio,
            ISyteLineServicio syteLineServicio,
            ISyteLineTokenServicio tokenServicio)
        {
            _sroRepositorio = sroRepositorio;
            _valeRepositorio = valeRepositorio;
            _detalleRepositorio = detalleRepositorio;
            _syteLineServicio = syteLineServicio;
            _tokenServicio = tokenServicio;
        }

        // ── 1. Buscar Vales en estado Pendiente ─────────────────────
        public async Task<ValeRecuperoListaViewModel> BuscarVales(ValeRecuperoBuscarDto filtros)
        {
            var vales = await _valeRepositorio.Buscar(
                filtros.NumeroSro,
                filtros.NumeroVale,
                filtros.CodigoArticuloReciclaje,
                filtros.DescripcionArticuloReciclaje,
                filtros.FechaVale);

            var dtos = vales.Select(v => new ValeRecuperoDto
            {
                ValeId = v.ValeId,
                NumeroVale = v.NumeroVale,
                ArticuloNoInventariado = v.Srolinea.ArticuloNoInv,
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
                CheckRecepcion = v.CheckRecepcion ?? false,
                Estado = v.Estado,
                FechaCreacion = v.FechaCreacionAudit
            }).ToList();

            return new ValeRecuperoListaViewModel
            {
                Filtros = filtros,
                Vales = dtos,
                BusquedaRealizada = true
            };
        }

        // ── 2. Guardar recepción ────────────────────────────────────
        public async Task GuardarRecepcion(List<ValeRecepcionDto> recepciones, string usuarioActual)
        {
            var recepcionesValidas = recepciones
                .Where(r => r.CheckRecepcion)
                .ToList();

            if (!recepcionesValidas.Any())
                return;

            // ── Obtener credenciales SyteLine una sola vez (caché interna del servicio) ──
            string? accessToken = null;
            string? mongooseConfig = null;
            try
            {
                var creds = await _tokenServicio.ObtenerCredencialesAsync();
                accessToken = creds.AccessToken;
                mongooseConfig = creds.MongooseConfig;
            }
            catch
            {
                // Si el token falla, continuamos sin consultar costo unitario
            }

            foreach (var rec in recepcionesValidas)
            {
                var vale = await _valeRepositorio.ObtenerPorId(rec.ValeId)
                    ?? throw new KeyNotFoundException($"ValeID {rec.ValeId} no encontrado.");

                // Solo se actualizan vales en estado Pendiente
                if (vale.Estado != "Pendiente")
                    continue;

                if (!string.IsNullOrWhiteSpace(rec.ArticuloReciclaje))
                    vale.ArticuloReciclaje = rec.ArticuloReciclaje;

                vale.CantidadRecibida = rec.CantidadRecibida;
                vale.PesoRecibido = rec.PesoRecibido;
                vale.CheckRecepcion = rec.CheckRecepcion;
                vale.FechaRecepcion = rec.CheckRecepcion ? DateTime.Now : null;
                vale.Estado = rec.CheckRecepcion ? "Recepcionado" : "Pendiente";
                vale.UsuarioModificacionAudit = usuarioActual;

                // ── Consultar CostoUnitario desde SyteLine ───────────────────────
                if (accessToken != null)
                {
                    try
                    {
                        var linea = vale.Srolinea
                                    ?? await _sroRepositorio.ObtenerLineaPorId(vale.SrolineaId);

                        if (linea != null && !string.IsNullOrWhiteSpace(linea.ArticuloNoInv))
                        {
                            var costo = await _syteLineServicio.GetCostoUnitarioAsync(
                                linea.ArticuloNoInv, accessToken, mongooseConfig ?? string.Empty);
                            vale.CostoUnitario = costo;
                        }
                    }
                    catch
                    {
                        // Si falla la consulta del costo, no bloqueamos la recepción
                    }
                }

                await _valeRepositorio.Actualizar(vale);

                // ── Distribuir cantidades y peso entre los detalles del vale ──
                await _detalleRepositorio.DistribuirRecepcion(
                    valeId: rec.ValeId,
                    cantidadRecibida: rec.CantidadRecibida ?? 0m,
                    pesoRecibido: rec.PesoRecibido ?? 0m,
                    checkRecepcion: rec.CheckRecepcion,
                    usuarioModificacion: usuarioActual);
            }
        }
    }
}
