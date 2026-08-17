using ComprobantePago.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComprobantePago.Infrastructure.Persistence.Configurations
{
    public class LugarPagoConfiguration : IEntityTypeConfiguration<LugarPago>
    {
        public void Configure(EntityTypeBuilder<LugarPago> builder)
        {
            builder.ToTable("rcolugarpago");
            builder.HasKey(x => x.IdLugarPago);
            builder.Property(x => x.Codigo).HasMaxLength(5).IsRequired();
            builder.Property(x => x.Descripcion).HasMaxLength(100).IsRequired();
            builder.Property(x => x.UsuarioReg).HasMaxLength(50).IsRequired();
        }
    }
}
