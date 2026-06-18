using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reciclaje.Dominio.Entidades;

namespace Reciclaje.Infraestructura.Data.Configuracion
{
    public class LogintegracionConfiguracion : IEntityTypeConfiguration<Logintegracionsyteline>
    {
        public void Configure(EntityTypeBuilder<Logintegracionsyteline> entidad)
        {
            entidad.ToTable("logintegracionsyteline");

            entidad.HasKey(e => e.LogId).HasName("PRIMARY");

            entidad.Property(e => e.LogId)
                .HasColumnType("int(11)")
                .HasColumnName("LogID");

            entidad.Property(e => e.EstadoEnvio)
                .HasMaxLength(50);

            entidad.Property(e => e.FechaEnvio)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");

            entidad.Property(e => e.Mensaje)
                .HasColumnType("text");

            entidad.Property(e => e.Trama)
                .HasColumnType("text");
        }
    }
}
