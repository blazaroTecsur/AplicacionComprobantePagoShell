namespace Resguardo.Domain.Entities;

public partial class Servicio : EntidadBase
{
    public int IdSolicitud { get; set; }
    public string Folio { get; set; } = null!;
    public int IdTpoServicio { get; set; }
    public DateOnly Fecha { get; set; }
    public string HraInicio { get; set; } = null!;
    public string HraFinal { get; set; } = null!;
    public string Turno { get; set; } = null!;
    public bool DiaSig { get; set; }
    public string Coordenada { get; set; } = null!;
    public string Direccion { get; set; } = null!;
    public int Cantidad { get; set; }
    public int? CantidadBck { get; set; }
    public string? Comentario { get; set; }    
    public virtual Solicitud SolicitudNav { get; set; } = null!;
    public virtual Generico TpoServicioNav { get; set; } = null!;
    public virtual ICollection<ServicioProv> ServicioProvs { get; set; } = new List<ServicioProv>();
}
