using ComprobantePago.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComprobantePago.Infrastructure.Persistence.Configurations
{
    public class MonedaConfiguration : IEntityTypeConfiguration<Moneda>
    {
        public void Configure(EntityTypeBuilder<Moneda> builder)
        {
            builder.ToTable("rcomoneda");
            builder.HasKey(x => x.IdMoneda);
            builder.Property(x => x.Codigo).HasMaxLength(5).IsRequired();
            builder.Property(x => x.Descripcion).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Simbolo).HasMaxLength(5);
            builder.Property(x => x.UsuarioReg).HasMaxLength(50).IsRequired();
        }
    }
}
