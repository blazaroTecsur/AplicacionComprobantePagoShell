using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reciclaje.Dominio.Entidades;

namespace Reciclaje.Infraestructura.Data.Configuracion;

public class IntegracionsytelineConfiguracion : IEntityTypeConfiguration<Integracionsyteline>
{
    public void Configure(EntityTypeBuilder<Integracionsyteline> e)
    {
        e.ToTable("integracionsyteline");
        e.HasKey(x => x.RowPointer);
        e.Property(x => x.RowPointer).HasMaxLength(36);
        e.Property(x => x.Sitio).HasMaxLength(10);
        e.Property(x => x.Sro).HasMaxLength(10);
        e.Property(x => x.Articulo).HasMaxLength(30);
        e.Property(x => x.Estado).HasMaxLength(10);
        e.Property(x => x.TransDate).HasColumnType("datetime");
        e.Property(x => x.FechaCreacion).HasColumnType("datetime");
        e.Property(x => x.FechaModificacion).HasColumnType("datetime");
    }
}