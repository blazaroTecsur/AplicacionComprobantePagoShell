using Microsoft.EntityFrameworkCore;
using Reciclaje.Dominio.Entidades;
using Reciclaje.Dominio.Interfaces;
using Reciclaje.Infraestructura.Data;

namespace Reciclaje.Infraestructura.Repositorios;

public class ValeRecuperoDetalleRepositorio : IValeRecuperoDetalleRepositorio
{
    private readonly DBContexto _context;

    public ValeRecuperoDetalleRepositorio(DBContexto context)
        => _context = context;

    // ── Insertar detalles al generar el vale ────────────────────────────────
    public async Task InsertarDetalles(IEnumerable<Valerecuperodetalle> detalles)
    {
        var now = DateTime.Now;
        var list = detalles.ToList();
        foreach (var d in list)
            d.FechaCreacionAudit = now;

        await _context.Valerecuperodetalles.AddRangeAsync(list);
        await _context.SaveChangesAsync();
    }

    // ── Obtener detalles por ValeID ─────────────────────────────────────────
    public async Task<IEnumerable<Valerecuperodetalle>> ObtenerPorValeId(int valeId) =>
        await _context.Valerecuperodetalles
            .Include(d => d.Srolinea)
                .ThenInclude(l => l.Sro)
            .Where(d => d.ValeId == valeId)
            .AsNoTracking()
            .ToListAsync();

    // ── Distribuir recepción entre las líneas del vale ──────────────────────
    /// <summary>
    /// Distribuye CantidadRecibida y PesoRecibido entre los detalles del vale:
    ///
    /// CantidadRecibida — llenado secuencial por DetalleId:
    ///   Cada línea recibe hasta su máximo (CantidadNoInv); el sobrante pasa
    ///   a la siguiente línea. La última línea acumula cualquier excedente.
    ///
    ///   Ej. recibido=35, líneas=[30,10] → detalle1=30, detalle2=5
    ///   Ej. recibido=40, líneas=[20,20] → detalle1=20, detalle2=20
    ///
    /// PesoRecibido — mismo valor exacto en todas las líneas.
    ///   Ej. peso=20, dos líneas → detalle1=20, detalle2=20
    /// </summary>
    public async Task DistribuirRecepcion(
        int valeId,
        decimal cantidadRecibida,
        decimal pesoRecibido,
        bool checkRecepcion,
        string usuarioModificacion)
    {
        var detalles = await _context.Valerecuperodetalles
            .Where(d => d.ValeId == valeId)
            .OrderByDescending(d => d.CantidadNoInv)  // mayor CantidadNoInv recibe primero
            .ThenBy(d => d.DetalleId)                 // desempate estable por ID
            .ToListAsync();

        if (!detalles.Any())
            return;

        var now = DateTime.Now;
        decimal restante = cantidadRecibida;

        for (int i = 0; i < detalles.Count; i++)
        {
            var detalle = detalles[i];
            decimal maximo = detalle.CantidadNoInv ?? 0m;
            bool esUltimo = (i == detalles.Count - 1);

            decimal asignado;

            if (esUltimo)
            {
                // La última línea recibe todo lo que queda
                // (puede ser 0 si ya se agotó, o el excedente si recibido > ΣCantNoInv)
                asignado = restante;
            }
            else
            {
                // Llenar hasta el máximo; el sobrante fluye a la siguiente línea
                asignado = Math.Min(restante, maximo);
                restante -= asignado;
            }

            detalle.CantidadRecibida = asignado;
            detalle.PesoRecibido = pesoRecibido;  // mismo valor en todas
            detalle.CheckRecepcion = checkRecepcion;
            detalle.FechaModificacionAudit = now;
            detalle.UsuarioModificacionAudit = usuarioModificacion;
        }

        await _context.SaveChangesAsync();
    }

    // ── Buscar detalles para el reporte, con filtros del vale padre ─────────
    public async Task<IEnumerable<Valerecuperodetalle>> BuscarParaReporte(
        string? numeroSro,
        string? numeroVale,
        string? codigoArticuloReciclaje,
        string? descripcionArticuloReciclaje,
        DateTime? fechaVale)
    {
        var query = _context.Valerecuperodetalles
            .Include(d => d.Valerecupero)
            .Include(d => d.Srolinea)
                .ThenInclude(l => l.Sro)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(numeroSro))
            query = query.Where(d => d.Srolinea.Sro.NumeroSro
                                      .Contains(numeroSro));

        if (!string.IsNullOrWhiteSpace(numeroVale))
            query = query.Where(d => d.Valerecupero.NumeroVale
                                      .Contains(numeroVale));

        if (!string.IsNullOrWhiteSpace(codigoArticuloReciclaje))
            query = query.Where(d => d.ArticuloReciclaje != null &&
                                     d.ArticuloReciclaje.Contains(codigoArticuloReciclaje));

        if (fechaVale.HasValue)
            query = query.Where(d => d.Valerecupero.FechaCreacionAudit.HasValue &&
                                     d.Valerecupero.FechaCreacionAudit.Value.Date == fechaVale.Value.Date);

        return await query
            .OrderBy(d => d.CodigoSupervisorNoInv)
            .ThenBy(d => d.Valerecupero.NumeroVale)
            .ThenBy(d => d.DetalleId)
            .AsNoTracking()
            .ToListAsync();
    }

}
