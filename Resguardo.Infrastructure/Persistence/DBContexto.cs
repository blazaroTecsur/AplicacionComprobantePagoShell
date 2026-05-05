using Microsoft.EntityFrameworkCore;
using Resguardo.Domain.Entities;
using System.Reflection;

namespace Resguardo.Infrastructure.Data;

public partial class DBContexto : DbContext
{
    public DBContexto()
    {
    }
    public DBContexto(DbContextOptions<DBContexto> options) : base(options)
    {
    }
    public virtual DbSet<Aprobador> Aprobador { get; set; }
    public virtual DbSet<Limite> Config { get; set; }    
    public virtual DbSet<Efectivo> Efectivo { get; set; }
    public virtual DbSet<Generico> Generico { get; set; }
    public virtual DbSet<Personal> Personal { get; set; }
    public virtual DbSet<Servicio> Servicio { get; set; }
    public virtual DbSet<Solicitud> Solicitud { get; set; }
    public virtual DbSet<ServicioProv> ServicioCtta { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("utf8_general_ci");
        //.HasCharSet("utf8");
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        modelBuilder.Entity<ServicioProv>()
        .Ignore(e => e.UsuarioReg)
        .Ignore(e => e.FechaReg)
        .Ignore(e => e.UsuarioAct)
        .Ignore(e => e.FechaAct);
    }
}
