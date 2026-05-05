using ComprobantePago.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComprobantePago.Infrastructure.Persistence.Configurations
{
    public class CodigoUnidad4Configuration
        : IEntityTypeConfiguration<CodigoUnidad4>
    {
        public void Configure(EntityTypeBuilder<CodigoUnidad4> builder)
        {
            builder.ToTable("tmacodigounidad4");
            builder.HasKey(x => x.IdCodigoUnidad4);
            builder.Property(x => x.Codigo).HasMaxLength(10).IsRequired();
            builder.Property(x => x.Descripcion).HasMaxLength(200).IsRequired();
            builder.Property(x => x.UsuarioReg).HasMaxLength(50).IsRequired();
        }
    }
}
