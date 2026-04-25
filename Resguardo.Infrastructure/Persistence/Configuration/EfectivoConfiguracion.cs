using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Resguardo.Domain.Entities;

namespace Resguardo.Infrastructure.Data.Configuracion
{
    internal class EfectivoConfiguracion : IEntityTypeConfiguration<Efectivo>
    {
        public void Configure(EntityTypeBuilder<Efectivo> entidad)
        {
            entidad.ToTable("rpoefectivo");
            entidad.HasKey(e => e.Id).HasName("PRIMARY");
            entidad.Property(e => e.Id).HasColumnName("IdEfectivo");

            entidad.HasOne(d => d.PersonalNav).WithMany(p => p.Efectivos)
                .HasForeignKey(d => d.IdPersonal)
                .OnDelete(DeleteBehavior.ClientSetNull);
            entidad.HasOne(d => d.ServicioProvNav).WithMany(p => p.Efectivos)
                .HasForeignKey(d => d.IdServicioProv)
                .OnDelete(DeleteBehavior.ClientSetNull);
        }
    }
}