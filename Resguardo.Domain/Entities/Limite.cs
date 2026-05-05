namespace Resguardo.Domain.Entities;

public partial class Limite : EntidadBase
{
    public DateOnly Fecha { get; set; }
    public string CodDpto { get; set; } = null!;
    public int IdTpoServicio { get; set; }
    public int Diurno { get; set; }
    public int Nocturno { get; set; }        
    public virtual Generico TpoServicioNav { get; set; } = null!;
}
