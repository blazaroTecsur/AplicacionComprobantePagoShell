using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Resguardo.Domain.Entities;

namespace Resguardo.Infrastructure.Data.Configuracion
{
    public class SolicitudConfiguracion : IEntityTypeConfiguration<Solicitud>
    {
        public void Configure(EntityTypeBuilder<Solicitud> entidad)
        {           
            entidad.ToTable("rposolicitud");
            entidad.HasKey(e => e.Id).HasName("PRIMARY");
            entidad.Property(e => e.Id).HasColumnName("IdSolicitud");

            entidad.HasOne(d => d.EstadoNav).WithMany(p => p.SolicitudEstados)
                .HasForeignKey(d => d.IdEstado)
                .OnDelete(DeleteBehavior.ClientSetNull);
            entidad.HasOne(d => d.FlujoNav).WithMany(p => p.SolicitudFlujos)
                .HasForeignKey(d => d.IdFlujo)
                .OnDelete(DeleteBehavior.ClientSetNull);
            entidad.HasOne(d => d.TipoNav).WithMany(p => p.SolicitudTipos)
                .HasForeignKey(d => d.IdTipo)
                .OnDelete(DeleteBehavior.ClientSetNull);

        }
    }
}