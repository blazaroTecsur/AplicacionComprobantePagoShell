using ComprobantePago.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComprobantePago.Infrastructure.Persistence.Configurations
{
    public class ProveedorConfiguration : IEntityTypeConfiguration<Proveedor>
    {
        public void Configure(EntityTypeBuilder<Proveedor> builder)
        {
            builder.ToTable("tmaproveedor");
            builder.HasKey(x => x.IdProveedor);
            builder.Property(x => x.NombreProveedor).HasMaxLength(255);
            builder.Property(x => x.TipoPersona).HasMaxLength(20).IsRequired();
            builder.Property(x => x.Direccion1).HasMaxLength(255);
            builder.Property(x => x.Direccion2).HasMaxLength(255);
            builder.Property(x => x.Direccion3).HasMaxLength(255);
            builder.Property(x => x.Direccion4).HasMaxLength(255);
            builder.Property(x => x.Comprador).HasMaxLength(150);
            builder.Property(x => x.Estado).HasMaxLength(20).IsRequired();
            builder.Property(x => x.Contacto).HasMaxLength(250);
            builder.Property(x => x.TelefonoContacto).HasMaxLength(20);
            builder.Property(x => x.CorreoExternoContacto).HasMaxLength(100);
            builder.Property(x => x.CorreoInternoContacto).HasMaxLength(100);
            builder.Property(x => x.Ruc).HasMaxLength(20).IsRequired();
            builder.Property(x => x.UsuarioReg).HasMaxLength(30);
            builder.Property(x => x.UsuarioAct).HasMaxLength(30);
            builder.HasIndex(x => x.IdProveedorExternal).IsUnique();
        }
    }
}
