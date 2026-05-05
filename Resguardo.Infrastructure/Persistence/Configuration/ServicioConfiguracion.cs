using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Resguardo.Domain.Entities;

namespace Resguardo.Infrastructure.Data.Configuracion
{
    public class ServicioConfiguracion : IEntityTypeConfiguration<Servicio>
    {
        public void Configure(EntityTypeBuilder<Servicio> entidad)
        {
            entidad.ToTable("rposervicio");
            entidad.HasKey(e => e.Id).HasName("PRIMARY");
            entidad.Property(e => e.Id).HasColumnName("IdServicio");

            entidad.HasOne(d => d.SolicitudNav).WithMany(p => p.Servicios)
                .HasForeignKey(d => d.IdSolicitud)
                .OnDelete(DeleteBehavior.ClientSetNull);
            entidad.HasOne(d => d.TpoServicioNav).WithMany(p => p.Servicios)
                .HasForeignKey(d => d.IdTpoServicio)
                .OnDelete(DeleteBehavior.ClientSetNull);
        }
    }
}