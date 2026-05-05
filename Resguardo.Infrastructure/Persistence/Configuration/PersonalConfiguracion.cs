using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Resguardo.Domain.Entities;

namespace Resguardo.Infrastructure.Data.Configuracion
{
    public class PersonalConfiguracion : IEntityTypeConfiguration<Personal>
    {
        public void Configure(EntityTypeBuilder<Personal> entidad)
        {
            entidad.ToTable("rpopersonal");
            entidad.HasKey(e => e.Id).HasName("PRIMARY");
            entidad.Property(e => e.Id).HasColumnName("IdPersonal");
        }
    }
}