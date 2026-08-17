using ComprobantePago.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComprobantePago.Infrastructure.Persistence.Configurations
{
    public class CodigoUnidad1Configuration
        : IEntityTypeConfiguration<CodigoUnidad1>
    {
        public void Configure(EntityTypeBuilder<CodigoUnidad1> builder)
        {
            builder.ToTable("tmacodigounidad1");
            builder.HasKey(x => x.IdCodigoUnidad1);
            builder.Property(x => x.Codigo).HasMaxLength(10).IsRequired();
            builder.Property(x => x.Descripcion).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Empresa).HasMaxLength(50).IsRequired();
            builder.Property(x => x.UsuarioReg).HasMaxLength(50).IsRequired();
        }
    }
}
