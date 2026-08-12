using Microsoft.EntityFrameworkCore;
using Reciclaje.Dominio.Entidades;
using Reciclaje.Dominio.Interfaces;
using Reciclaje.Infraestructura.Data;

namespace Reciclaje.Infraestructura.Repositorios
{
    public class TareaOrdenCompraRepositorio : ITareaOrdenCompraRepositorio
    {
        private readonly DBContexto _context;

        public TareaOrdenCompraRepositorio(DBContexto context)
        {
            _context = context;
        }

        // ── Obtener todos, orden descendente por año y mes ───────────
        public async Task<IEnumerable<Tareaordencompra>> ObtenerTodos()
        {
            return await _context.Tareaordencompras
                .AsNoTracking()
                .OrderByDescending(t => t.Anno)
                .ThenByDescending(t => t.Mes)
                .ToListAsync();
        }

        // ── Obtener por ID ───────────────────────────────────────────
        public async Task<Tareaordencompra?> ObtenerPorId(int id)
        {
            return await _context.Tareaordencompras
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        // ── Obtener por NombrePo ─────────────────────────────────────
        public async Task<Tareaordencompra?> ObtenerPorNombrePo(string nombrePo)
        {
            return await _context.Tareaordencompras
                .FirstOrDefaultAsync(t => t.NombrePo == nombrePo);
        }

        // ── Verificar unicidad anno + mes + sitio ────────────────────
        public async Task<bool> ExistePorAnnoMesSitio(short anno, byte mes, string? sitio, int? excluirId = null)
        {
            var query = _context.Tareaordencompras
                .Where(t => t.Anno == anno && t.Mes == mes);

            // Comparación null-safe para sitio
            query = sitio == null
                ? query.Where(t => t.Sitio == null)
                : query.Where(t => t.Sitio == sitio);

            if (excluirId.HasValue)
                query = query.Where(t => t.Id != excluirId.Value);

            return await query.AnyAsync();
        }

        // ── Verificar si existe registro para el periodo actual (año/mes) ──
        public async Task<bool> ExistePeriodoActual()
        {
            var ahora = DateTime.Now;
            var anno = (short)ahora.Year;
            var mes = (byte)ahora.Month;

            return await _context.Tareaordencompras
                .AnyAsync(t => t.Anno == anno && t.Mes == mes);
        }

        // ── Insertar ─────────────────────────────────────────────────
        public async Task Insertar(Tareaordencompra tarea)
        {
            await _context.Tareaordencompras.AddAsync(tarea);
            await _context.SaveChangesAsync();
        }

        // ── Actualizar ───────────────────────────────────────────────
        public async Task Actualizar(Tareaordencompra tarea)
        {
            _context.Tareaordencompras.Update(tarea);
            await _context.SaveChangesAsync();
        }

        // ── Eliminar ─────────────────────────────────────────────────
        public async Task Eliminar(Tareaordencompra tarea)
        {
            _context.Tareaordencompras.Remove(tarea);
            await _context.SaveChangesAsync();
        }
    }
}