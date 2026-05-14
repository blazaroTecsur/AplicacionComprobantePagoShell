using ComprobantePago.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComprobantePago.Infrastructure.Persistence.Configurations
{
    public class CuentaContableConfiguration
        : IEntityTypeConfiguration<CuentaContable>
    {
        public void Configure(EntityTypeBuilder<CuentaContable> builder)
        {
            builder.ToTable("tmacuentacontable");
            builder.HasKey(x => x.IdCuentaContable);
            builder.Property(x => x.Codigo).HasColumnName("Cuenta").HasMaxLength(20).IsRequired();
            builder.Property(x => x.Descripcion).HasMaxLength(200).IsRequired();
            builder.Ignore(x => x.Activo);
            builder.Property(x => x.UsuarioReg).HasMaxLength(50).IsRequired();
        }
    }
}
