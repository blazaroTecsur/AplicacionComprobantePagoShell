using Microsoft.EntityFrameworkCore;
using Reciclaje.Dominio.Entidades;
using Reciclaje.Dominio.Interfaces;
using Reciclaje.Infraestructura.Data;

namespace Reciclaje.Infraestructura.Repositorios
{
    public class ValeRecuperoRepositorio : IValeRecuperoRepositorio
    {
        private readonly DBContexto _context;

        public ValeRecuperoRepositorio(DBContexto context)
        {
            _context = context;
        }

        // ── Genera número correlativo: VALE-2026-00001 ──────────────
        public async Task<string> GenerarNumeroVale()
        {
            var anio = DateTime.Now.Year;
            var prefijo = $"VALE-{anio}-";

            var ultimo = await _context.Valerecuperos
                .Where(v => v.NumeroVale.StartsWith(prefijo))
                .OrderByDescending(v => v.NumeroVale)
                .Select(v => v.NumeroVale)
                .FirstOrDefaultAsync();

            int correlativo = 1;
            if (ultimo != null)
            {
                var partes = ultimo.Split('-');
                if (partes.Length == 3 && int.TryParse(partes[2], out int ultimo_num))
                    correlativo = ultimo_num + 1;
            }

            return $"{prefijo}{correlativo:D5}";
        }

        public async Task InsertarVale(Valerecupero vale)
        {
            vale.FechaCreacionAudit = DateTime.Now;
            await _context.Valerecuperos.AddAsync(vale);
            await _context.SaveChangesAsync();
        }

        public async Task InsertarVales(IEnumerable<Valerecupero> vales)
        {
            var now = DateTime.Now;
            foreach (var v in vales)
                v.FechaCreacionAudit = now;

            await _context.Valerecuperos.AddRangeAsync(vales);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Valerecupero>> Buscar(
            string? numeroSro,
            string? numeroVale,
            string? codigoArticuloReciclaje,
            string? descripcionArticuloReciclaje,
            DateTime? fechaVale)
        {
            var query = _context.Valerecuperos
                .Include(v => v.Srolinea)
                    .ThenInclude(l => l.Sro)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(numeroSro))
                query = query.Where(v => v.Srolinea.Sro.NumeroSro.Contains(numeroSro));

            if (!string.IsNullOrWhiteSpace(numeroVale))
                query = query.Where(v => v.NumeroVale.Contains(numeroVale));

            if (!string.IsNullOrWhiteSpace(codigoArticuloReciclaje))
                query = query.Where(v => v.Srolinea.ArticuloReciclaje != null &&
                    v.Srolinea.ArticuloReciclaje.Contains(codigoArticuloReciclaje));

            if (!string.IsNullOrWhiteSpace(descripcionArticuloReciclaje))
                query = query.Where(v => v.Srolinea.DescripcionAlmacenNoInv != null &&
                    v.Srolinea.DescripcionAlmacenNoInv.Contains(descripcionArticuloReciclaje));

            if (fechaVale.HasValue)
                query = query.Where(v => v.FechaCreacionAudit.HasValue &&
                    v.FechaCreacionAudit.Value.Date == fechaVale.Value.Date);

            return await query
                .OrderByDescending(v => v.FechaCreacionAudit)
                .ToListAsync();
        }

        public async Task<Valerecupero?> ObtenerPorId(int valeId)
        {
            return await _context.Valerecuperos
                .Include(v => v.Srolinea)
                    .ThenInclude(l => l.Sro)
                .FirstOrDefaultAsync(v => v.ValeId == valeId);
        }

        public async Task Actualizar(Valerecupero vale)
        {
            vale.FechaModificacionAudit = DateTime.Now;
            _context.Valerecuperos.Update(vale);
            await _context.SaveChangesAsync();
        }


    }
}