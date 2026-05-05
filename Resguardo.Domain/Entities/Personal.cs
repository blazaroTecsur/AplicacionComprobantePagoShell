namespace Resguardo.Domain.Entities;

public partial class Personal : EntidadBase
{
    public string Dni { get; set; } = null!;
    public string Nombres { get; set; } = null!;
    public string Apellidos { get; set; } = null!;
    public string? Telefono { get; set; }
    public virtual ICollection<Efectivo> Efectivos { get; set; } = new List<Efectivo>();
}
