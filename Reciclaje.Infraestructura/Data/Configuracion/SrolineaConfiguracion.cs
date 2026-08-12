using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reciclaje.Dominio.Entidades;

namespace Reciclaje.Infraestructura.Data.Configuracion

{
    public class SrolineaConfiguracion : IEntityTypeConfiguration<Srolinea>
    {
        public void Configure(EntityTypeBuilder<Srolinea> entidad)
        {
            entidad.ToTable("srolinea");

            entidad.HasKey(e => e.SrolineaId).HasName("PRIMARY");

            entidad.HasIndex(e => e.Sroid, "SROID");

            entidad.HasIndex(e => e.SroLineaSL, "SROLineaSL");

            entidad.Property(e => e.SrolineaId)
                .HasColumnType("int(11)")
                .HasColumnName("SROLineaID");

            entidad.Property(e => e.ArticuloNoInv).HasMaxLength(10);

            entidad.Property(e => e.CantidadNoInv)
                .HasPrecision(18, 4);

            entidad.Property(e => e.CodigoAlmacenNoInv)
                .HasMaxLength(50);

            entidad.Property(e => e.CodigoSupervisorNoInv)
                .HasMaxLength(50);

            entidad.Property(e => e.DescripcionAlmacenNoInv)
                .HasMaxLength(255);

            entidad.Property(e => e.DescripcionSupervisorNoInv)
                .HasMaxLength(255);

            entidad.Property(e => e.ArticuloReciclaje)
                .HasMaxLength(10);

            entidad.Property(e => e.EstadoLinea)
                .HasMaxLength(50);

            entidad.Property(e => e.FechaCreacionAudit)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");

            entidad.Property(e => e.FechaModificacionAudit)
                .HasColumnType("datetime");

            entidad.Property(e => e.FechaTransaccion)
                .HasColumnType("datetime");

            entidad.Property(e => e.OrdenCompra)
                .HasMaxLength(50);

            entidad.Property(e => e.Sroid)
                .HasColumnType("int(11)")
                .HasColumnName("SROID");

            entidad.Property(e => e.SroLineaSL)
                .HasColumnType("int(11)")
                .HasColumnName("SROLineaSL");

            entidad.Property(e => e.UmnoInv)
                .HasMaxLength(10)
                .HasColumnName("UMNoInv");

            entidad.Property(e => e.UsuarioCreacionAudit)
                .HasMaxLength(50);

            entidad.Property(e => e.UsuarioModificacionAudit)
                .HasMaxLength(50);

            entidad.Property(e => e.UMReciclaje)
                .HasMaxLength(10);

            entidad.Property(e => e.RowPointer).HasMaxLength(36);

            entidad.Property(e => e.TramaSyteLine)
                .HasColumnName("TramaSyteLine")
                .HasColumnType("longtext");

            entidad.HasOne(d => d.Sro)
                .WithMany(p => p.Srolineas)
                .HasForeignKey(d => d.Sroid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("srolinea_ibfk_1");

            entidad.Property(e => e.ConversionID)
                .HasColumnType("int(11)")
                .HasColumnName("ConversionID")
                .IsRequired(false); // Permite nulos

            entidad.Property(e => e.Dept)
                .HasMaxLength(50)
                .HasColumnName("Dept");

            entidad.HasOne(d => d.Conversionarticulo)
                .WithMany(p => p.Srolineas)
                .HasForeignKey(d => d.ConversionID)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_srolinea_conversionarticulo");
        }
    }
}