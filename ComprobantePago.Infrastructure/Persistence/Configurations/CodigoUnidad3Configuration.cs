using ComprobantePago.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComprobantePago.Infrastructure.Persistence.Configurations
{
    public class CodigoUnidad3Configuration
        : IEntityTypeConfiguration<CodigoUnidad3>
    {
        public void Configure(EntityTypeBuilder<CodigoUnidad3> builder)
        {
            builder.ToTable("tmacodigounidad3");
            builder.HasKey(x => x.IdCodigoUnidad3);
            builder.Property(x => x.Codigo).HasMaxLength(10).IsRequired();
            builder.Property(x => x.Descripcion).HasMaxLength(200).IsRequired();
            builder.Property(x => x.UsuarioReg).HasMaxLength(50).IsRequired();
        }
    }
}
