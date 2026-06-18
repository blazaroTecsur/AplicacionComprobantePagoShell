using Microsoft.EntityFrameworkCore;
using Reciclaje.Dominio.Entidades;
using Reciclaje.Dominio.Interfaces;
using Reciclaje.Infraestructura.Data;

namespace Reciclaje.Infraestructura.Repositorios
{
    public class SroRepositorio : ISroRepositorio
    {
        private readonly DBContexto _context;

        public SroRepositorio(DBContexto context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Srolinea>> BuscarLineas(
            string? numeroSro,
            DateTime? fechaTransaccion,
            string? articuloNoInv,
            string? descripcionArticulo)
        {
            var query = _context.Srolineas
                .Include(l => l.Sro)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(numeroSro))
                query = query.Where(l => l.Sro.NumeroSro.Contains(numeroSro));

            if (fechaTransaccion.HasValue)
                query = query.Where(l => l.FechaTransaccion.HasValue &&
                    l.FechaTransaccion.Value.Date == fechaTransaccion.Value.Date);

            if (!string.IsNullOrWhiteSpace(articuloNoInv))
                query = query.Where(l => l.ArticuloNoInv.Contains(articuloNoInv));

            // descripcionArticulo filtra por DescripcionAlmacenNoInv como aproximación
            if (!string.IsNullOrWhiteSpace(descripcionArticulo))
                query = query.Where(l => l.DescripcionAlmacenNoInv != null &&
                    l.DescripcionAlmacenNoInv.Contains(descripcionArticulo));

            return await query
                .OrderBy(l => l.Sro.NumeroSro)
                .ThenBy(l => l.FechaTransaccion)
                .ToListAsync();
        }

        // ── NUEVO ──────────────────────────────────────
        public async Task<Srolinea?> ObtenerLineaPorId(int srolineaId)
        {
            return await _context.Srolineas
                .Include(l => l.Sro)
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.SrolineaId == srolineaId);
        }

        public async Task ActualizarEstadoLinea(int srolineaId, string estadoLinea, string usuarioModificacion)
        {
            var linea = await _context.Srolineas
                .FirstOrDefaultAsync(l => l.SrolineaId == srolineaId);

            if (linea is null) return;

            linea.EstadoLinea = estadoLinea;
            linea.FechaModificacionAudit = DateTime.Now;
            linea.UsuarioModificacionAudit = usuarioModificacion;

            await _context.SaveChangesAsync();
        }
    }
}