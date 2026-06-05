using ComprobantePago.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComprobantePago.Infrastructure.Persistence.Configurations
{
    public class SerieCorrelativoConfiguration
            : IEntityTypeConfiguration<SerieCorrelativo>
    {
        public void Configure(EntityTypeBuilder<SerieCorrelativo> builder)
        {
            builder.ToTable("rcoseriecorrelativo");
            builder.HasKey(x => x.IdSerieCorrelativo);
            builder.Property(x => x.CodigoEmpresa).HasMaxLength(50).IsRequired();
            builder.Property(x => x.TipoDocumento).HasMaxLength(10).IsRequired();
            builder.Property(x => x.Anio).IsRequired();
            builder.Property(x => x.Mes).IsRequired();
            builder.Property(x => x.UltimoCorrelativo).IsRequired();

            builder.HasIndex(x => new { x.CodigoEmpresa, x.TipoDocumento, x.Anio, x.Mes })
                   .IsUnique()
                   .HasDatabaseName("uq_serie");
        }
    }
}
