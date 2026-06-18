using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reciclaje.Dominio.Entidades;

namespace Reciclaje.Infraestructura.Data.Configuracion
{
    internal class ConversionarticuloConfiguracion : IEntityTypeConfiguration<Conversionarticulo>
    {
        public void Configure(EntityTypeBuilder<Conversionarticulo> entidad)
        {
            entidad.ToTable("conversionarticulo");

            entidad.HasKey(e => e.ConversionId).HasName("PRIMARY");

            entidad.Property(e => e.ConversionId)
                .HasColumnType("int(11)")
                .HasColumnName("ConversionID");

            entidad.Property(e => e.ArticuloNoInventariado).HasMaxLength(10);
            entidad.Property(e => e.ArticuloReciclaje).HasMaxLength(10);

            entidad.Property(e => e.DescripcionArticuloReciclaje)
                .HasMaxLength(50);

            entidad.Property(e => e.FechaCreacionAudit)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");

            entidad.Property(e => e.FechaModificacionAudit)
                .HasColumnType("datetime");

            entidad.Property(e => e.UsuarioCreacionAudit)
                .HasMaxLength(50);

            entidad.Property(e => e.UsuarioModificacionAudit)
                .HasMaxLength(50);

            entidad.HasMany(e => e.Srolineas)
                .WithOne(s => s.Conversionarticulo)
                .HasForeignKey(s => s.ConversionID)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_srolinea_conversionarticulo");

        }
    }
}
