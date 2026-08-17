using ComprobantePago.Application.Mapping;
using ComprobantePago.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ComprobantePago.Tests.Helpers
{
    public static class DbContextFactory
    {
        private static bool _mapsterConfigurado;
        private static readonly object _lock = new();

        public static AppDbContext Crear(string? nombre = null)
        {
            // Mapster necesita estar configurado antes de que Adapt<> se use.
            // En tests no pasa por Program.cs, así que lo inicializamos aquí.
            if (!_mapsterConfigurado)
            {
                lock (_lock)
                {
                    if (!_mapsterConfigurado)
                    {
                        MapsterConfig.Configure();
                        _mapsterConfigurado = true;
                    }
                }
            }

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(nombre ?? Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }
    }
}
