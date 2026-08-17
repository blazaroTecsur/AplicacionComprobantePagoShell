using ComprobantePago.Application.Interfaces;
using ComprobantePago.Infrastructure.Persistence;

namespace ComprobantePago.Tests.Helpers
{
    /// <summary>
    /// UnitOfWork para pruebas con BD InMemory.
    /// SaveChangesAsync usa el DbContext real; las transacciones son no-op
    /// porque EF Core InMemory no las soporta.
    /// </summary>
    public sealed class TestUnitOfWork(AppDbContext db) : IUnitOfWork
    {
        private readonly AppDbContext _db = db;

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);

        public Task BeginTransactionAsync() => Task.CompletedTask;
        public Task CommitAsync()           => Task.CompletedTask;
        public Task RollbackAsync()         => Task.CompletedTask;
        public void Dispose()               { }
    }
}
