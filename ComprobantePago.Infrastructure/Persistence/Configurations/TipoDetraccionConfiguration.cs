using ComprobantePago.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComprobantePago.Infrastructure.Persistence.Configurations
{
    public class TipoDetraccionConfiguration
        : IEntityTypeConfiguration<TipoDetraccion>
    {
        public void Configure(EntityTypeBuilder<TipoDetraccion> builder)
        {
            builder.ToTable("rcotipodetraccion");
            builder.HasKey(x => x.IdTipoDetraccion);
            builder.Property(x => x.Codigo).HasMaxLength(5).IsRequired();
            builder.Property(x => x.Descripcion).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Porcentaje).HasColumnType("decimal(5,2)");
            builder.Property(x => x.UsuarioReg).HasMaxLength(50).IsRequired();
        }
    }
}
