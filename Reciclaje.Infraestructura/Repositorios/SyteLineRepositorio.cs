using Microsoft.EntityFrameworkCore;
using Reciclaje.Dominio.Entidades;
using Reciclaje.Dominio.Interfaces;
using Reciclaje.Infraestructura.Data;

namespace Reciclaje.Infraestructura.Repositorios;

public class SyteLineRepositorio(DBContexto context) : ISyteLineRepositorio
{
    // ── integracionsyteline ──────────────────────────────────────────────────

    public async Task<bool> ExisteIntegracion(string rowPointer) =>
        await context.Integracionsytelines
            .AnyAsync(l => l.RowPointer == rowPointer);  // PK directo, más eficiente

    public async Task InsertarIntegracion(Integracionsyteline registro)
    {
        await context.Integracionsytelines.AddAsync(registro);
        await context.SaveChangesAsync();
    }

    // ── sro ──────────────────────────────────────────────────────────────────

    public async Task<Sro?> ObtenerSroPorNumero(string numeroSro) =>
        await context.Sros.FirstOrDefaultAsync(s => s.NumeroSro == numeroSro);

    public async Task InsertarSro(Sro sro)
    {
        await context.Database.ExecuteSqlRawAsync(@"
        INSERT IGNORE INTO sro (NumeroSRO, Sitio, FechaCreacionAudit)
        VALUES ({0}, {1}, {2})",
        sro.NumeroSro, sro.Sitio ?? (object)DBNull.Value, sro.FechaCreacionAudit ?? (object)DBNull.Value);
    }

    // ── srolinea ─────────────────────────────────────────────────────────────

    public async Task<bool> ExisteSrolinea(string rowPointer) =>
        await context.Srolineas.AnyAsync(l => l.RowPointer == rowPointer);

    public async Task InsertarSrolinea(Srolinea linea)
    {
        await context.Database.ExecuteSqlRawAsync(@"
    INSERT IGNORE INTO srolinea 
        (SROID, SROLineaSL, RowPointer, CodigoAlmacenNoInv, ArticuloNoInv,
         UMNoInv, CantidadNoInv, FechaTransaccion, ArticuloReciclaje,
         UMReciclaje, TramaSyteLine, OrdenCompra, ConversionID, Dept, EstadoLinea, FechaCreacionAudit)
    VALUES ({0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15})",
         linea.Sroid,
         linea.SroLineaSL,
         linea.RowPointer ?? (object)DBNull.Value,
         linea.CodigoAlmacenNoInv ?? (object)DBNull.Value,
         linea.ArticuloNoInv,
         linea.UmnoInv ?? (object)DBNull.Value,
         linea.CantidadNoInv.HasValue ? (object)linea.CantidadNoInv.Value : DBNull.Value,
         linea.FechaTransaccion.HasValue ? (object)linea.FechaTransaccion.Value : DBNull.Value,
         linea.ArticuloReciclaje ?? (object)DBNull.Value,
         linea.UMReciclaje ?? (object)DBNull.Value,
         linea.TramaSyteLine ?? (object)DBNull.Value,
         linea.OrdenCompra ?? (object)DBNull.Value,
         linea.ConversionID.HasValue ? (object)linea.ConversionID.Value : DBNull.Value,
         linea.Dept ?? (object)DBNull.Value,
         linea.EstadoLinea ?? (object)DBNull.Value,
         linea.FechaCreacionAudit.HasValue ? (object)linea.FechaCreacionAudit.Value : DBNull.Value);
    }

    public async Task<IEnumerable<Srolinea>> ObtenerLineasConVales(IEnumerable<int>? sroLineaIds)
    {
        var query = context.Srolineas
            .Include(l => l.Valerecuperos)
            .AsNoTracking()
            .Where(l => l.TramaSyteLine != null && l.Valerecuperos.Any());

        var ids = sroLineaIds?.ToList();
        if (ids is { Count: > 0 })
            query = query.Where(l => ids.Contains(l.SrolineaId));

        return await query.OrderBy(l => l.SrolineaId).ToListAsync();
    }

    // ── conversionarticulo ───────────────────────────────────────────────────

    public async Task<(string? ArticuloReciclaje, int? ConversionId)> ObtenerArticuloReciclaje(string articuloNoInventariado)
    {
        var conv = await context.Conversionarticulos
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.ArticuloNoInventariado == articuloNoInventariado);

        return (conv?.ArticuloReciclaje, conv?.ConversionId);
    }

    public async Task<int> ObtenerSroidPorNumero(string numeroSro) =>
    await context.Sros
        .Where(s => s.NumeroSro == numeroSro)
        .Select(s => s.Sroid)
        .FirstOrDefaultAsync();

    // ── tareaordencompra ─────────────────────────────────────────────────────

    public async Task<string?> ObtenerNombrePoAsync(short anno, byte mes) =>
        await context.Tareaordencompras
            .AsNoTracking()
            .Where(t => t.Anno == anno && t.Mes == mes)
            .Select(t => t.NombrePo)
            .FirstOrDefaultAsync();

}