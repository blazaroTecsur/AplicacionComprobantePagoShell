using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reciclaje.Dominio.Entidades;

namespace Reciclaje.Infraestructura.Data.Configuracion;

public class ValerecuperodetalleConfiguracion : IEntityTypeConfiguration<Valerecuperodetalle>
{
    public void Configure(EntityTypeBuilder<Valerecuperodetalle> entidad)
    {
        entidad.ToTable("valerecupero_detalle");

        entidad.HasKey(e => e.DetalleId).HasName("PRIMARY");

        entidad.HasIndex(e => e.RowPointer).IsUnique().HasDatabaseName("RowPointer");
        entidad.HasIndex(e => e.ValeId,     "FK_vrd_idvale");
        entidad.HasIndex(e => e.SrolineaId, "FK_vrd_srolineaid");

        // ── Columnas ────────────────────────────────────────────────────────
        entidad.Property(e => e.DetalleId)
            .HasColumnName("IdValRecuperoDetalle")
            .HasColumnType("int");

        entidad.Property(e => e.ValeId)
            .HasColumnName("ValeID")
            .HasColumnType("int");

        entidad.Property(e => e.SrolineaId)
            .HasColumnName("SROLineaID")
            .HasColumnType("int");

        entidad.Property(e => e.SroId)
            .HasColumnName("SROID")
            .HasColumnType("int");

        entidad.Property(e => e.SroLineaSL)
            .HasColumnName("SROLineaSL")
            .HasColumnType("int");

        entidad.Property(e => e.ArticuloNoInv)
            .HasColumnName("ArticuloNoInv")
            .HasMaxLength(10);

        entidad.Property(e => e.CantidadNoInv)
            .HasColumnName("CantidadNoInv")
            .HasPrecision(18, 4);

        entidad.Property(e => e.UmnoInv)
            .HasColumnName("UMNoInv")
            .HasMaxLength(10);

        entidad.Property(e => e.FechaTransaccion)
            .HasColumnName("FechaTransaccion")
            .HasColumnType("datetime");

        entidad.Property(e => e.CodigoAlmacenNoInv)
            .HasColumnName("CodigoAlmacenNoInv")
            .HasMaxLength(50);

        entidad.Property(e => e.DescripcionAlmacenNoInv)
            .HasColumnName("DescripcionAlmacenNoInv")
            .HasMaxLength(255);

        entidad.Property(e => e.CodigoSupervisorNoInv)
            .HasColumnName("CodigoSupervisorNoInv")
            .HasMaxLength(50);

        entidad.Property(e => e.DescripcionSupervisorNoInv)
            .HasColumnName("DescripcionSupervisorNoInv")
            .HasMaxLength(255);

        entidad.Property(e => e.ArticuloReciclaje)
            .HasColumnName("ArticuloReciclaje")
            .HasMaxLength(10);

        entidad.Property(e => e.UMReciclaje)
            .HasColumnName("UMReciclaje")
            .HasMaxLength(10);

        entidad.Property(e => e.OrdenCompra)
            .HasColumnName("OrdenCompra")
            .HasMaxLength(50);

        entidad.Property(e => e.EstadoLinea)
            .HasColumnName("EstadoLinea")
            .HasMaxLength(50);

        entidad.Property(e => e.FechaCreacionAudit)
            .HasColumnName("FechaCreacionAudit")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnType("datetime");

        entidad.Property(e => e.UsuarioCreacionAudit)
            .HasColumnName("UsuarioCreacionAudit")
            .HasMaxLength(50);

        entidad.Property(e => e.FechaModificacionAudit)
            .HasColumnName("FechaModificacionAudit")
            .HasColumnType("datetime");

        entidad.Property(e => e.UsuarioModificacionAudit)
            .HasColumnName("UsuarioModificacionAudit")
            .HasMaxLength(50);

        entidad.Property(e => e.RowPointer)
            .HasColumnName("RowPointer")
            .HasMaxLength(36)
            .IsRequired();

        entidad.Property(e => e.RowPointer)
            .HasPrecision(18, 4);

        entidad.Property(e => e.RowPointer)
            .HasPrecision(18, 4);

        entidad.Property(e => e.RowPointer)
            .HasDefaultValueSql("'0'");

        // ── Relaciones ───────────────────────────────────────────────────────
        entidad.HasOne(d => d.Valerecupero)
            .WithMany()
            .HasForeignKey(d => d.ValeId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_vrd_idvale");

        entidad.HasOne(d => d.Srolinea)
            .WithMany()
            .HasForeignKey(d => d.SrolineaId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_vrd_srolineaid");
    }
}
