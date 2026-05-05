using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Resguardo.Domain.Entities;

namespace Resguardo.Infrastructure.Data.Configuracion
{
    internal class LimiteConfiguracion : IEntityTypeConfiguration<Limite>
    {
        public void Configure(EntityTypeBuilder<Limite> entidad)
        {
            entidad.ToTable("rpoconfig");
            entidad.HasKey(e => e.Id).HasName("PRIMARY");
            entidad.Property(e => e.Id).HasColumnName("IdConfig");

            entidad.HasOne(d => d.TpoServicioNav).WithMany(p => p.Configs)
                .HasForeignKey(d => d.IdTpoServicio)
                .OnDelete(DeleteBehavior.ClientSetNull);
        }
    }
}