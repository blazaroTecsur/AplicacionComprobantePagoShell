using ComprobantePago.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComprobantePago.Infrastructure.Persistence.Configurations
{
    public class TipoSunatConfiguration
        : IEntityTypeConfiguration<TipoSunat>
    {
        public void Configure(EntityTypeBuilder<TipoSunat> builder)
        {
            builder.ToTable("rcotiposunat");
            builder.HasKey(x => x.IdTipoSunat);
            builder.Property(x => x.Codigo).HasMaxLength(5).IsRequired();
            builder.Property(x => x.Descripcion).HasMaxLength(100).IsRequired();
            builder.Property(x => x.UsuarioReg).HasMaxLength(50).IsRequired();
        }
    }
}
