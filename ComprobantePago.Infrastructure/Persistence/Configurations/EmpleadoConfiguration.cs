using ComprobantePago.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComprobantePago.Infrastructure.Persistence.Configurations
{
    public class EmpleadoConfiguration : IEntityTypeConfiguration<Empleado>
    {
        public void Configure(EntityTypeBuilder<Empleado> builder)
        {
            builder.ToTable("tmaempleado");
            builder.HasKey(x => x.IdEmpleado);

            builder.Property(x => x.IdEmpleadoExternal).HasMaxLength(20).IsRequired();
            builder.Property(x => x.Codigo).HasMaxLength(20).IsRequired();
            builder.Property(x => x.NombreCompleto).HasMaxLength(200).IsRequired();

            builder.Property(x => x.Apellido).HasMaxLength(150);
            builder.Property(x => x.Nombre).HasMaxLength(150);
            builder.Property(x => x.Alias).HasMaxLength(150);
            builder.Property(x => x.Cargo).HasMaxLength(150);
            builder.Property(x => x.Departamento).HasMaxLength(20);
            builder.Property(x => x.Estado).HasMaxLength(20);
            builder.Property(x => x.Turno).HasMaxLength(20);
            builder.Property(x => x.Categoria).HasMaxLength(20);
            builder.Property(x => x.IdUsuario).HasMaxLength(50);
            builder.Property(x => x.FrecuenciaPago).HasMaxLength(20);
            builder.Property(x => x.TipoEmpleado).HasMaxLength(50);
            builder.Property(x => x.GeneraNomina).HasMaxLength(50);
            builder.Property(x => x.CuentaSueldo).HasMaxLength(50);

            builder.Property(x => x.PrimerNombre).HasMaxLength(100);
            builder.Property(x => x.SegundoNombre).HasMaxLength(100);
            builder.Property(x => x.PrimerApellido).HasMaxLength(100);
            builder.Property(x => x.SegundoApellido).HasMaxLength(100);

            builder.Property(x => x.Direccion1).HasMaxLength(200);
            builder.Property(x => x.Direccion2).HasMaxLength(200);
            builder.Property(x => x.Ciudad).HasMaxLength(100);
            builder.Property(x => x.CodProvincia).HasMaxLength(10);
            builder.Property(x => x.CP).HasMaxLength(20);
            builder.Property(x => x.Telefono).HasMaxLength(50);
            builder.Property(x => x.CorreoElect).HasMaxLength(200);

            builder.HasIndex(x => x.IdEmpleadoExternal).IsUnique();
        }
    }
}
