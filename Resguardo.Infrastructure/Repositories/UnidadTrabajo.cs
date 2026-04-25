using Microsoft.EntityFrameworkCore.Storage;
using Resguardo.Domain.Interfaces;
using Resguardo.Domain.Repositories;
using Resguardo.Infrastructure.Data;
using Resguardo.Infrastructure.Repositories;

namespace Resguardo.Infrastructure.Repositorios
{
    public class UnidadTrabajo : IUnidadTrabajo
    {
        private readonly DBContexto _contexto;
        private IDbContextTransaction? _transaction;
        private bool _disposed = false;
        private readonly ISolicitudRepositorio _solicitud;
        private readonly IServicioRepositorio _servicio;
        private readonly IEfectivoRepositorio _efectivo;
        private readonly IPersonalRepositorio _personal;
        private readonly IGenericoRepositorio _generico;
        private readonly IServicioProvRepositorio _servicioProv;
        private readonly IConfigRepositorio _config;
        public UnidadTrabajo(DBContexto contexto)
        {
            _contexto = contexto;
        }
        public ISolicitudRepositorio SolicitudRepositorio => _solicitud ?? new SolicitudRepositorio(_contexto);
        public IServicioRepositorio ServicioRepositorio => _servicio ?? new ServicioRepositorio(_contexto);
        public IEfectivoRepositorio EfectivoRepositorio => _efectivo ?? new EfectivoRepositorio(_contexto);
        public IPersonalRepositorio PersonalRepositorio => _personal ?? new PersonalRepositorio(_contexto);
        public IGenericoRepositorio GenericoRepositorio => _generico ?? new GenericoRepositorio(_contexto);
        public IServicioProvRepositorio ServicioProvRepositorio => _servicioProv ?? new ServicioProvRepositorio(_contexto);
        public IConfigRepositorio ConfigRepositorio => _config ?? new ConfigRepositorio(_contexto);
        public async Task BeginTransactionAsync()
        {
            // ✅ No permitir transacciones anidadas — lanzar en lugar de descartar
            if (_transaction != null)
                throw new InvalidOperationException(
                    "Ya existe una transacción activa. Commit o Rollback antes de iniciar otra.");

            _transaction = await _contexto.Database.BeginTransactionAsync();
        }
        public async Task CommitTransactionAsync()
        {
            if (_transaction == null)
                throw new InvalidOperationException(
                    "No hay transacción activa para confirmar.");
            try
            {
                await _contexto.SaveChangesAsync();   // ✅ Guardar antes de commitear
                await _transaction.CommitAsync();
            }
            catch
            {
                await RollbackTransactionAsync();     // ✅ Rollback automático si falla
                throw;                                // Re-lanzar para que el caller sepa
            }
            finally
            {
                await DisposeTransactionAsync();
            }
        }
        public async Task RollbackTransactionAsync()
        {
            if (_transaction == null) return;         // ✅ Silencioso si no hay transacción

            try
            {
                await _transaction.RollbackAsync();
            }
            finally
            {
                await DisposeTransactionAsync();
            }
        }        
        public async Task SaveChangesAsync()
        {
            await _contexto.SaveChangesAsync();
        }        
        private async Task DisposeTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            _disposed = true;

            // ✅ Si queda transacción abierta al disponer → es un bug del caller
            // Hacemos rollback pero también logueamos/advertimos
            if (_transaction != null)
            {
                // Considera loguear: _logger.LogWarning("UoW disposed con transacción activa")
                await _transaction.RollbackAsync();
                await DisposeTransactionAsync();
            }

            await _contexto.DisposeAsync();
            GC.SuppressFinalize(this);
        }
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            // ✅ Evitar .Wait() — usar el patrón correcto para Dispose síncrono
            if (_transaction != null)
            {
                // Rollback síncrono disponible directamente
                _transaction.Rollback();
                _transaction.Dispose();
                _transaction = null;
            }

            _contexto.Dispose();
            GC.SuppressFinalize(this);
        }        
    }
}
