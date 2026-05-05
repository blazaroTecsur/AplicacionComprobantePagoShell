namespace Resguardo.Domain.Entities;

public partial class Generico : EntidadBase
{
    public string Tipo { get; set; } = null!;
    public string Codigo { get; set; } = null!;
    public string Descripcion { get; set; } = null!;
    public virtual ICollection<Limite> Configs { get; set; } = new List<Limite>();
    public virtual ICollection<Servicio> Servicios { get; set; } = new List<Servicio>();
    public virtual ICollection<Solicitud> SolicitudEstados { get; set; } = new List<Solicitud>();
    public virtual ICollection<Solicitud> SolicitudFlujos { get; set; } = new List<Solicitud>();
    public virtual ICollection<Solicitud> SolicitudTipos { get; set; } = new List<Solicitud>();
    public virtual ICollection<ServicioProv> ServicioProvs { get; set; } = new List<ServicioProv>();
}