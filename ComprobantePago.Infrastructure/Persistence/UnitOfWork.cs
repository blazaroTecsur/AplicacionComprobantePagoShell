using ComprobantePago.Application.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace ComprobantePago.Infrastructure.Persistence
{
    public class UnitOfWork(AppDbContext contexto) : IUnitOfWork
    {
        private readonly AppDbContext _contexto = contexto;
        private IDbContextTransaction? _transaction;

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => _contexto.SaveChangesAsync(cancellationToken);

        public async Task BeginTransactionAsync()
            => _transaction = await _contexto.Database.BeginTransactionAsync();

        // Envuelve el bloque completo (begin + trabajo + commit/rollback) dentro de
        // CreateExecutionStrategy para ser compatible con EnableRetryOnFailure.
        public async Task ExecuteInTransactionAsync(Func<Task> action)
        {
            var strategy = _contexto.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _contexto.Database.BeginTransactionAsync();
                try
                {
                    await action();
                    await tx.CommitAsync();
                }
                catch
                {
                    await tx.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task CommitAsync()
        {
            if (_transaction is null) return;
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        public async Task RollbackAsync()
        {
            if (_transaction is null) return;
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        public void Dispose() => _transaction?.Dispose();
    }
}
