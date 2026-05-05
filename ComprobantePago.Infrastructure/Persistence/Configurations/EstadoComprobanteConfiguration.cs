using ComprobantePago.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComprobantePago.Infrastructure.Persistence.Configurations
{
    public class EstadoComprobanteConfiguration
        : IEntityTypeConfiguration<EstadoComprobante>
    {
        public void Configure(EntityTypeBuilder<EstadoComprobante> builder)
        {
            builder.ToTable("rcoestadocomprobante");
            builder.HasKey(x => x.IdEstadoComprobante);
            builder.Property(x => x.Codigo).HasMaxLength(20).IsRequired();
            builder.Property(x => x.Descripcion).HasMaxLength(100).IsRequired();
            builder.Property(x => x.UsuarioReg).HasMaxLength(50).IsRequired();
        }
    }
}
