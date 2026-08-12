using Microsoft.EntityFrameworkCore;
using Reciclaje.Dominio.Entidades;
using System.Reflection;

namespace Reciclaje.Infraestructura.Data;

public partial class DBContexto : DbContext
{

    public DBContexto()
    {
    }
    public DBContexto(DbContextOptions<DBContexto> options) : base(options)
    {
    }

    public virtual DbSet<Conversionarticulo> Conversionarticulos { get; set; }

    public virtual DbSet<Logintegracionsyteline> Logintegracionsytelines { get; set; }

    public virtual DbSet<Sro> Sros { get; set; }

    public virtual DbSet<Srolinea> Srolineas { get; set; }

    public virtual DbSet<Valerecupero> Valerecuperos { get; set; }

    public virtual DbSet<Integracionsyteline> Integracionsytelines { get; set; }

    public virtual DbSet<Tareaordencompra> Tareaordencompras { get; set; }

    public virtual DbSet<Valerecuperodetalle> Valerecuperodetalles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("utf8mb4_general_ci")
            .HasCharSet("utf8mb4");
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
