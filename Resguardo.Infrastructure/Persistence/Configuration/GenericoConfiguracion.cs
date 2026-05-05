using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Resguardo.Domain.Entities;

namespace Resguardo.Infrastructure.Data.Configuracion
{
    internal class GenericoConfiguracion : IEntityTypeConfiguration<Generico>
    {
        public void Configure(EntityTypeBuilder<Generico> entidad)
        {
            entidad.ToTable("rpogenerico");
            entidad.HasKey(e => e.Id).HasName("PRIMARY");
            entidad.Property(e => e.Id).HasColumnName("IdGenerico");
        }
    }
}
