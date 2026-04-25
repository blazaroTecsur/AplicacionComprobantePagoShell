using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Resguardo.Domain.Entities;

namespace Resguardo.Infrastructure.Data.Configuracion
{
    public class AprobadorConfiguracion : IEntityTypeConfiguration<Aprobador>
    {
        public void Configure(EntityTypeBuilder<Aprobador> entidad)
        {
            entidad.ToTable("rpoaprobador");
            entidad.HasKey(e => e.Id).HasName("PRIMARY");
            entidad.Property(e => e.Id).HasColumnName("IdAprobador");            
        }
    }
}
