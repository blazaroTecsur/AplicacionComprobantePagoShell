using ComprobantePago.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComprobantePago.Infrastructure.Persistence.Configurations
{
    public class TipoDocumentoConfiguration
        : IEntityTypeConfiguration<TipoDocumento>
    {
        public void Configure(EntityTypeBuilder<TipoDocumento> builder)
        {
            builder.ToTable("rcotipodocumento");
            builder.HasKey(x => x.IdTipoDocumento);
            builder.Property(x => x.Codigo).HasMaxLength(5).IsRequired();
            builder.Property(x => x.Descripcion).HasMaxLength(100).IsRequired();
            builder.Property(x => x.UsuarioReg).HasMaxLength(50).IsRequired();
        }
    }
}
