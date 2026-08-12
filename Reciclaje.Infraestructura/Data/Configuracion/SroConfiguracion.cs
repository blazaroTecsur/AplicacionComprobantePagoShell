using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reciclaje.Dominio.Entidades;

namespace Reciclaje.Infraestructura.Data.Configuracion

{
    public class SroConfiguracion : IEntityTypeConfiguration<Sro>
    {
        public void Configure(EntityTypeBuilder<Sro> entidad)
        {
            entidad.ToTable("sro");

            entidad.HasKey(e => e.Sroid).HasName("PRIMARY");

            entidad.HasIndex(e => e.NumeroSro, "NumeroSRO").IsUnique();

            entidad.Property(e => e.Sroid)
                .HasColumnType("int(11)")
                .HasColumnName("SROID");

            entidad.Property(e => e.CodigoSubcontratista)
                .HasMaxLength(50);

            entidad.Property(e => e.DescripcionSubcontratista)
                .HasMaxLength(255);

            entidad.Property(e => e.Estado)
                .HasMaxLength(50);

            entidad.Property(e => e.FechaCreacionAudit)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");

            entidad.Property(e => e.FechaModificacionAudit)
                .HasColumnType("datetime");

            entidad.Property(e => e.NumeroSro)
                .HasMaxLength(50)
                .HasColumnName("NumeroSRO");

            entidad.Property(e => e.Ruc)
                .HasMaxLength(20)
                .HasColumnName("RUC");

            entidad.Property(e => e.Sitio)
                .HasMaxLength(50);

            entidad.Property(e => e.UsuarioCreacionAudit)
                .HasMaxLength(50);

            entidad.Property(e => e.UsuarioModificacionAudit)
                .HasMaxLength(50);
        }
    }
}