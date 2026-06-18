using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reciclaje.Dominio.Entidades;

namespace Reciclaje.Infraestructura.Data.Configuracion
{
    public class ValerecuperoConfiguracion : IEntityTypeConfiguration<Valerecupero>
    {
        public void Configure(EntityTypeBuilder<Valerecupero> entidad)
        {
            entidad.ToTable("valerecupero");

            entidad.HasKey(e => e.ValeId).HasName("PRIMARY");

            entidad.HasIndex(e => e.NumeroVale, "NumeroVale").IsUnique();

            entidad.HasIndex(e => e.SrolineaId, "SROLineaID");

            entidad.Property(e => e.ValeId)
                .HasColumnType("int(11)")
                .HasColumnName("ValeID");

            entidad.Property(e => e.ArticuloReciclaje)
                .IsRequired()
                .HasMaxLength(10)
                .HasColumnName("ArticuloReciclaje");

            entidad.Property(e => e.ArticuloReciclajeId)
                .HasColumnType("int(11)")
                .HasColumnName("ArticuloReciclajeID");

            entidad.Property(e => e.CantidadRecibida)
                .HasPrecision(18, 4);

            entidad.Property(e => e.CheckConfirmacion)
                .HasDefaultValueSql("'0'");

            entidad.Property(e => e.CheckRecepcion)
                .HasDefaultValueSql("'0'");

            entidad.Property(e => e.Estado)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Pendiente'");

            entidad.Property(e => e.FechaConfirmacion)
                .HasColumnType("datetime");

            entidad.Property(e => e.FechaCreacionAudit)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");

            entidad.Property(e => e.FechaModificacionAudit)
                .HasColumnType("datetime");

            entidad.Property(e => e.FechaRecepcion)
                .HasColumnType("datetime");

            entidad.Property(e => e.NumeroVale)
                .HasMaxLength(50);

            entidad.Property(e => e.Ocanual)
                .HasMaxLength(50)
                .HasColumnName("OCAnual");

            entidad.Property(e => e.PesoRecibido)
                .HasPrecision(18, 4);

            entidad.Property(e => e.CantidadReciclaje)
                .HasPrecision(18, 4);

            entidad.Property(e => e.SrolineaId)
                .HasColumnType("int(11)")
                .HasColumnName("SROLineaID");

            entidad.Property(e => e.TipoVale)
                .HasMaxLength(20);

            entidad.Property(e => e.Umreciclaje)
                .HasMaxLength(10)
                .HasDefaultValueSql("'Kg'")
                .HasColumnName("UMReciclaje");

            entidad.Property(e => e.UsuarioConfirmacionId)
                .HasColumnType("int(11)")
                .HasColumnName("UsuarioConfirmacionID");

            entidad.Property(e => e.UsuarioCreacionAudit)
                .HasMaxLength(50);

            entidad.Property(e => e.UsuarioModificacionAudit)
                .HasMaxLength(50);

            entidad.Property(e => e.UsuarioRecepcionId)
                .HasColumnType("int(11)")
                .HasColumnName("UsuarioRecepcionID");

            entidad.Property(e => e.CostoUnitario).HasPrecision(18, 5);

            entidad.HasOne(d => d.Srolinea)
                .WithMany(p => p.Valerecuperos)
                .HasForeignKey(d => d.SrolineaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("valerecupero_ibfk_1");
        }
    }
}