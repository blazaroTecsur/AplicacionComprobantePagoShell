using ComprobantePago.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComprobantePago.Infrastructure.Persistence.Configurations
{
    public class ImputacionContableConfiguration
        : IEntityTypeConfiguration<ImputacionContable>
    {
        public void Configure(EntityTypeBuilder<ImputacionContable> builder)
        {
            builder.ToTable("rcoimputacioncontable");
            builder.HasKey(x => x.IdImputacionContable);
            builder.Property(x => x.Folio).HasMaxLength(20).IsRequired();
            builder.Property(x => x.Monto).HasColumnType("decimal(18,2)");
            builder.Property(x => x.UsuarioReg).HasMaxLength(50).IsRequired();
        }
    }
}
