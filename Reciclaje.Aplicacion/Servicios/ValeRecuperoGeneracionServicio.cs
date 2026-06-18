using Reciclaje.Aplicacion.DTOs.Reciclaje;
using Reciclaje.Aplicacion.Interfaces;
using Reciclaje.Dominio.Entidades;
using Reciclaje.Dominio.Interfaces;

namespace Reciclaje.Aplicacion.Servicios;

/// <summary>
/// Implementa la lógica de búsqueda de líneas SRO y generación
/// de Vale Específico / Consolidado (vista Index).
/// Al crear un vale, registra además el detalle de cada SROLinea
/// seleccionada en la tabla valerecupero_detalle.
/// </summary>
public class ValeRecuperoGeneracionServicio : IValeRecuperoGeneracionServicio
{
    private readonly ISroRepositorio _sroRepositorio;
    private readonly IValeRecuperoRepositorio _valeRepositorio;
    private readonly IValeRecuperoDetalleRepositorio _detalleRepositorio;

    public ValeRecuperoGeneracionServicio(
        ISroRepositorio sroRepositorio,
        IValeRecuperoRepositorio valeRepositorio,
        IValeRecuperoDetalleRepositorio detalleRepositorio)
    {
        _sroRepositorio = sroRepositorio;
        _valeRepositorio = valeRepositorio;
        _detalleRepositorio = detalleRepositorio;
    }

    // ── 1. Buscar líneas SRO ─────────────────────────────────────────────────
    public async Task<ValeRecuperoViewModel> Buscar(ValeRecuperoBusquedaDto filtros)
    {
        var lineas = await _sroRepositorio.BuscarLineas(
            filtros.NumeroSro,
            filtros.FechaTransaccion,
            filtros.ArticuloNoInv,
            filtros.DescripcionArticulo);

        var dtos = lineas.Select(l => new SroLineaValeDto
        {
            SrolineaId = l.SrolineaId,
            NumeroSro = l.Sro.NumeroSro,
            SroLineaSL = l.SroLineaSL,
            ArticuloNoInv = l.ArticuloNoInv,
            CantidadNoInv = l.CantidadNoInv,
            UmnoInv = l.UmnoInv,
            CodigoSupervisorNoInv = l.CodigoSupervisorNoInv,
            Ruc = l.Sro.Ruc,
            DescripcionSubcontratista = l.Sro.DescripcionSubcontratista,
            OrdenCompra = l.OrdenCompra,
            EstadoSro = l.Sro.Estado,
            ArticuloReciclaje = l.ArticuloReciclaje,
            DescripcionAlmacenNoInv = l.DescripcionAlmacenNoInv,
            FechaTransaccion = l.FechaTransaccion,
            UMReciclaje = l.UMReciclaje,
            EstadoLinea = l.EstadoLinea
        }).ToList();

        return new ValeRecuperoViewModel
        {
            Filtros = filtros,
            Lineas = dtos,
            BusquedaRealizada = true
        };
    }

    // ── 2. Vale ESPECÍFICO: un NumeroVale por SST (NumeroSro) ───────────────
    /// <summary>
    /// Agrupa las líneas seleccionadas por NumeroSro (SST) y genera un
    /// <see cref="Valerecupero"/> por cada SST distinta.
    /// Registra en valerecupero_detalle CADA línea original del grupo
    /// y marca todas las líneas del grupo como Procesado.
    /// Ejemplo: si se seleccionan líneas de SST0000955 y SST0000956,
    /// se crean 2 vales — uno por cada SST.
    /// </summary>
    public async Task GenerarValeEspecifico(List<int> srolineaIds, string usuarioActual)
    {
        // Cargar todas las líneas seleccionadas con su SRO (NumeroSro)
        var lineas = new List<Srolinea>();
        foreach (var lineaId in srolineaIds)
        {
            var linea = await _sroRepositorio.ObtenerLineaPorId(lineaId)
                ?? throw new KeyNotFoundException($"SROLineaID {lineaId} no encontrado.");
            lineas.Add(linea);
        }

        // Agrupar por NumeroSro (SST): SST0000955, SST0000956, etc.
        var grupos = lineas
            .GroupBy(l => l.Sro?.NumeroSro ?? string.Empty)
            .ToList();

        foreach (var grupo in grupos)
        {
            var numeroVale = await _valeRepositorio.GenerarNumeroVale();
            var referencia = grupo.First();

            var vale = new Valerecupero
            {
                NumeroVale = numeroVale,
                TipoVale = "Especifico",
                SrolineaId = referencia.SrolineaId,        // línea representativa del grupo
                ArticuloReciclaje = referencia.ArticuloReciclaje ?? string.Empty,
                Umreciclaje = referencia.UMReciclaje ?? "Kg",
                CantidadReciclaje = grupo.Sum(l => l.CantidadNoInv ?? 0m),
                Ocanual = referencia.OrdenCompra,
                Estado = "Pendiente",
                CheckRecepcion = false,
                CheckConfirmacion = false,
                UsuarioCreacionAudit = usuarioActual
            };

            await _valeRepositorio.InsertarVale(vale);

            // Registrar CADA línea del grupo en valerecupero_detalle
            var detalles = grupo.Select(l => BuildDetalle(vale.ValeId, l, usuarioActual));
            await _detalleRepositorio.InsertarDetalles(detalles);

            // Marcar CADA línea del grupo como Procesado
            foreach (var linea in grupo)
                await _sroRepositorio.ActualizarEstadoLinea(linea.SrolineaId, "Procesado", usuarioActual);
        }
    }

    // ── 3. Vale CONSOLIDADO: un NumeroVale por grupo artículo/UM ────────────
    /// <summary>
    /// Agrupa las líneas seleccionadas por ArticuloNoInv + UmnoInv,
    /// crea un <see cref="Valerecupero"/> por cada grupo con la suma de cantidades,
    /// y registra en <c>valerecupero_detalle</c> CADA línea original del grupo.
    /// </summary>
    public async Task GenerarValeConsolidado(List<int> srolineaIds, string usuarioActual)
    {
        // Cargar todas las líneas seleccionadas
        var lineas = new List<Srolinea>();
        foreach (var lineaId in srolineaIds)
        {
            var linea = await _sroRepositorio.ObtenerLineaPorId(lineaId)
                ?? throw new KeyNotFoundException($"SROLineaID {lineaId} no encontrado.");
            lineas.Add(linea);
        }

        // Agrupar por ArticuloNoInv + UmnoInv
        var grupos = lineas
            .GroupBy(l => new
            {
                ArticuloNoInv = l.ArticuloNoInv ?? string.Empty,
                UmnoInv = l.UmnoInv ?? string.Empty
            })
            .ToList();

        foreach (var grupo in grupos)
        {
            var numeroVale = await _valeRepositorio.GenerarNumeroVale();
            var referencia = grupo.First();

            var vale = new Valerecupero
            {
                NumeroVale = numeroVale,
                TipoVale = "Consolidador",
                SrolineaId = referencia.SrolineaId,        // línea representativa
                ArticuloReciclaje = referencia.ArticuloReciclaje ?? string.Empty,
                CantidadReciclaje = grupo.Sum(l => l.CantidadNoInv ?? 0m),
                Umreciclaje = referencia.UMReciclaje ?? "Kg",
                Ocanual = referencia.OrdenCompra,
                Estado = "Pendiente",
                CheckRecepcion = false,
                CheckConfirmacion = false,
                UsuarioCreacionAudit = usuarioActual
            };

            await _valeRepositorio.InsertarVale(vale);

            // Registrar CADA línea del grupo en valerecupero_detalle
            var detalles = grupo.Select(l => BuildDetalle(vale.ValeId, l, usuarioActual));
            await _detalleRepositorio.InsertarDetalles(detalles);

            // Marcar CADA línea del grupo como Procesado
            foreach (var linea in grupo)
                await _sroRepositorio.ActualizarEstadoLinea(linea.SrolineaId, "Procesado", usuarioActual);
        }
    }

    // ── Validación: retorna identificadores de líneas ya Procesadas ──────────
    public async Task<List<string>> ObtenerLineasProcesadas(List<int> srolineaIds)
    {
        var resultado = new List<string>();
        foreach (var id in srolineaIds)
        {
            var linea = await _sroRepositorio.ObtenerLineaPorId(id);
            if (linea?.EstadoLinea == "Procesado")
                resultado.Add($"SRO {linea.Sro?.NumeroSro} - Línea {linea.SroLineaSL}");
        }
        return resultado;
    }

    // ── Helper: construir detalle desde una Srolinea ─────────────────────────
    private static Valerecuperodetalle BuildDetalle(int valeId, Srolinea linea, string usuarioActual) =>
        new()
        {
            ValeId = valeId,
            SrolineaId = linea.SrolineaId,
            SroId = linea.Sroid,
            SroLineaSL = linea.SroLineaSL,
            ArticuloNoInv = linea.ArticuloNoInv,
            CantidadNoInv = linea.CantidadNoInv,
            UmnoInv = linea.UmnoInv,
            FechaTransaccion = linea.FechaTransaccion,
            CodigoAlmacenNoInv = linea.CodigoAlmacenNoInv,
            DescripcionAlmacenNoInv = linea.DescripcionAlmacenNoInv,
            CodigoSupervisorNoInv = linea.CodigoSupervisorNoInv,
            DescripcionSupervisorNoInv = linea.DescripcionSupervisorNoInv,
            ArticuloReciclaje = linea.ArticuloReciclaje,
            UMReciclaje = linea.UMReciclaje,
            OrdenCompra = linea.OrdenCompra,
            EstadoLinea = linea.EstadoLinea,
            UsuarioCreacionAudit = usuarioActual,
            RowPointer = Guid.NewGuid().ToString()
        };
}
