using ComprobantePago.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComprobantePago.Infrastructure.Persistence.Configurations
{
    public class DocumentoElectronicoConfiguration
            : IEntityTypeConfiguration<DocumentoElectronico>
    {
        public void Configure(EntityTypeBuilder<DocumentoElectronico> builder)
        {
            builder.ToTable("rcodocumentoelectronico");
            builder.HasKey(x => x.IdDocumento);
            builder.Property(x => x.Folio).HasMaxLength(20).IsRequired();
            builder.Property(x => x.TipoArchivo).HasMaxLength(10).IsRequired();
            builder.Property(x => x.SubTipo).HasMaxLength(30).IsRequired().HasDefaultValue("");
            builder.Property(x => x.NombreArchivo).HasMaxLength(255).IsRequired();
            builder.Property(x => x.Contenido).HasColumnType("longblob").IsRequired();
            builder.Property(x => x.UsuarioReg).HasMaxLength(50).IsRequired();
            builder.HasIndex(x => x.Folio);
        }
    }
}
