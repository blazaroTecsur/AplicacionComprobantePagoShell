namespace Resguardo.Domain.Entities;

public partial class Efectivo : EntidadBase
{
    public int IdServicioProv { get; set; }    
    public int IdPersonal { get; set; }
    public string? Telefono { get; set; }
    public string? HraInicio { get; set; }
    public string? HraFinal { get; set; }
    public bool? Asistio { get; set; }
    public string? SroRefencia { get; set; }
    public string? Comentario { get; set; }
    public string? HraAmplia { get; set; }
    public string? EstAmplia { get; set; }
    public string? UsuarioApro { get; set; }
    public DateTime? FechaApro { get; set; }
    public string? ComentApro { get; set; }
    public virtual Personal PersonalNav { get; set; } = null!;
    public virtual ServicioProv ServicioProvNav { get; set; } = null!;
}
