using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reciclaje.Dominio.Entidades;

namespace Reciclaje.Infraestructura.Data.Configuracion
{
    public class TareaordencompraConfiguracion : IEntityTypeConfiguration<Tareaordencompra>
    {
        public void Configure(EntityTypeBuilder<Tareaordencompra> entidad)
        {
            entidad.ToTable("tareaordencompra");

            entidad.HasKey(e => e.Id).HasName("PK_TareaOrdenCompra");

            entidad.HasIndex(e => new { e.Anno, e.Mes, e.Sitio })
                .IsUnique()
                .HasDatabaseName("UQ_TareaOrdenCompra_AnnoMesSitio");

            entidad.Property(e => e.Id)
                .HasColumnName("id");

            entidad.Property(e => e.Anno)
                .HasColumnName("anno")
                .HasColumnType("smallint");

            entidad.Property(e => e.Mes)
                .HasColumnName("mes")
                .HasColumnType("tinyint unsigned");

            entidad.Property(e => e.NombrePo)
                .HasColumnName("nombrepo")
                .HasMaxLength(255);

            entidad.Property(e => e.Sitio)
                .HasColumnName("sitio")
                .HasMaxLength(100);

            entidad.Property(e => e.FechaCreacion)
                .HasColumnName("fechacreacion");

            entidad.Property(e => e.UsuarioCreacion)
                .HasColumnName("usuariocreacion")
                .HasMaxLength(100);

            entidad.Property(e => e.FechaModificacion)
                .HasColumnName("fechamodificacion");

            entidad.Property(e => e.UsuarioModificacion)
                .HasColumnName("usuariomodificacion")
                .HasMaxLength(100);

            entidad.Property(e => e.Estado)
                .HasColumnName("estado")
                .HasMaxLength(15);

            entidad.Property(e => e.UidSyteLine)
                .HasColumnName("uidsyteLine")
                .HasMaxLength(36);        // varchar(36) en lugar de char(36)

            entidad.Property(e => e.UltimaLinea)
                .HasColumnName("ultimalinea");
        }
    }
}