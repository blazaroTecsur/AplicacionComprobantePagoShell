using ComprobantePago.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ComprobantePago.Infrastructure.Persistence
{
    public class AppDbContext(DbContextOptions<AppDbContext> options)
            : DbContext(options)
    {
        // Catálogos
        public DbSet<TipoDocumento> TiposDocumento { get; set; }
        public DbSet<TipoSunat> TiposSunat { get; set; }
        public DbSet<Moneda> Monedas { get; set; }
        public DbSet<LugarPago> LugaresPago { get; set; }
        public DbSet<TipoDetraccion> TiposDetraccion { get; set; }
        public DbSet<EstadoComprobante> EstadosComprobante { get; set; }
        public DbSet<Empleado> Empleados { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<CuentaContable> CuentasContables { get; set; }
        public DbSet<CodigoUnidad1> CodigosUnidad1 { get; set; }
        public DbSet<CodigoUnidad3> CodigosUnidad3 { get; set; }
        public DbSet<CodigoUnidad4> CodigosUnidad4 { get; set; }

        // Transaccionales
        public DbSet<Comprobante> Comprobantes { get; set; }
        public DbSet<ImputacionContable> ImputacionesContables { get; set; }
        public DbSet<DocumentoElectronico> DocumentosElectronicos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(AppDbContext).Assembly);
        }
    }
}
