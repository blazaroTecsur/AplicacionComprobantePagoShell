using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Resguardo.Domain.Entities;

namespace Resguardo.Infrastructure.Persistence.Configuration
{
    public class ServicioProvConfiguracion : IEntityTypeConfiguration<ServicioProv>
    {
        public void Configure(EntityTypeBuilder<ServicioProv> entidad)
        {
            entidad.ToTable("rposervicioprov");
            entidad.HasKey(e => e.Id).HasName("PRIMARY");
            entidad.Property(e => e.Id).HasColumnName("IdServicioProv");

            entidad.HasOne(d => d.ProveedorNav).WithMany(p => p.ServicioProvs)
                .HasForeignKey(d => d.IdProveedor)
                .OnDelete(DeleteBehavior.ClientSetNull);
            entidad.HasOne(d => d.ServicioNav).WithMany(p => p.ServicioProvs)
                .HasForeignKey(d => d.IdServicio)
                .OnDelete(DeleteBehavior.ClientSetNull);
        }
    }
}
