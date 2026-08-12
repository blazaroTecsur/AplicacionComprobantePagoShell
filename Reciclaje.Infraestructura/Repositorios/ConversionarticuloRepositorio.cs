using Microsoft.EntityFrameworkCore;
using Reciclaje.Dominio.Entidades;
using Reciclaje.Dominio.Interfaces;
using Reciclaje.Infraestructura.Data;

namespace Reciclaje.Infraestructura.Repositorios
{
    public class ConversionarticuloRepositorio : IConversionarticuloRepositorio<Conversionarticulo>
    {
        private readonly DBContexto _context;

        public ConversionarticuloRepositorio(DBContexto context)
        {
            _context = context;
        }

        public async Task<Conversionarticulo> ObtenerPorId(int id)
        {
            return await _context.Conversionarticulos
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ConversionId == id)
                ?? throw new KeyNotFoundException($"ConversionID {id} no encontrado.");
        }

        public async Task Insertar(Conversionarticulo entidad)
        {
            entidad.FechaCreacionAudit = DateTime.Now;
            await _context.Conversionarticulos.AddAsync(entidad);
            await _context.SaveChangesAsync();
        }

        public void Actualizar(Conversionarticulo entidad)
        {
            entidad.FechaModificacionAudit = DateTime.Now;
            _context.Conversionarticulos.Update(entidad);
            _context.SaveChanges();
        }

        public async Task Eliminar(int id)
        {
            var entidad = await ObtenerPorId(id);
            _context.Conversionarticulos.Remove(entidad);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Conversionarticulo>> Listar()
        {
            return await _context.Conversionarticulos
                .AsNoTracking()
                .OrderBy(c => c.ConversionId)
                .ToListAsync();
        }

        public async Task<Conversionarticulo?> ObtenerPorArticuloReciclaje(string codigoArticuloReciclaje)
        {
            return await _context.Conversionarticulos
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ArticuloReciclaje == codigoArticuloReciclaje);
        }
    }
}