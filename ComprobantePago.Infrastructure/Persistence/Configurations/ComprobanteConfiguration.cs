using ComprobantePago.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComprobantePago.Infrastructure.Persistence.Configurations
{
    public class ComprobanteConfiguration
            : IEntityTypeConfiguration<Comprobante>
    {
        public void Configure(EntityTypeBuilder<Comprobante> builder)
        {
            builder.ToTable("rcocomprobante");
            builder.HasKey(x => x.IdComprobante);
            builder.Property(x => x.Folio).HasMaxLength(20).IsRequired();
            builder.HasIndex(x => x.Folio).IsUnique();
            builder.Property(x => x.RucReceptor).HasMaxLength(11).IsRequired();
            builder.Property(x => x.RazonSocialReceptor).HasMaxLength(200).IsRequired();
            builder.Property(x => x.TipoDocumento).HasMaxLength(5).IsRequired();
            builder.Property(x => x.TipoSunat).HasMaxLength(5).IsRequired();
            builder.Property(x => x.Serie).HasMaxLength(10).IsRequired();
            builder.Property(x => x.Numero).HasMaxLength(20).IsRequired();
            builder.Property(x => x.Moneda).HasMaxLength(5).IsRequired();
            builder.Property(x => x.TasaCambio).HasColumnType("decimal(10,4)");
            builder.Property(x => x.MontoNeto).HasColumnType("decimal(18,2)");
            builder.Property(x => x.MontoExento).HasColumnType("decimal(18,2)");
            builder.Property(x => x.PorcentajeIGV).HasColumnType("decimal(5,2)");
            builder.Property(x => x.MontoIGVCosto).HasColumnType("decimal(18,2)");
            builder.Property(x => x.MontoIGVCredito).HasColumnType("decimal(18,2)");
            builder.Property(x => x.MontoTotal).HasColumnType("decimal(18,2)");
            builder.Property(x => x.MontoBruto).HasColumnType("decimal(18,2)");
            builder.Property(x => x.MontoRetencion).HasColumnType("decimal(18,2)");
            builder.Property(x => x.MontoMultas).HasColumnType("decimal(18,2)");
            builder.Property(x => x.ValorAduana).HasColumnType("decimal(18,2)");
            builder.Property(x => x.MontoRedondeo).HasColumnType("decimal(18,2)");
            builder.Property(x => x.MontoDetraccion).HasColumnType("decimal(18,2)");
            builder.Property(x => x.CodigoEstado).HasMaxLength(20).IsRequired();
            builder.Property(x => x.UsuarioReg).HasMaxLength(50).IsRequired();
            builder.Property(x => x.UsuarioAct).HasMaxLength(50);
            builder.Property(x => x.EmpleadoCodigo).HasMaxLength(20);
            builder.Property(x => x.EmpleadoNombre).HasMaxLength(200);
            builder.Property(x => x.VoucherSyteline);

            // Relación
            builder.HasMany(x => x.Imputaciones)
                   .WithOne(x => x.Comprobante)
                   .HasForeignKey(x => x.Folio)
                   .HasPrincipalKey(x => x.Folio);
        }
    }
}
