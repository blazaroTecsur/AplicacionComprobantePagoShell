namespace Resguardo.Domain.Entities;

public partial class Solicitud : EntidadBase
{
    public string Folio { get; set; } = null!;
    public int IdTipo { get; set; }
    public int IdEstado { get; set; }
    public int IdFlujo { get; set; }
    public string NumSro { get; set; } = null!;
    public string CodDpto { get; set; } = null!;
    public string NomDpto { get; set; } = null!;
    public string CodActv { get; set; } = null!;
    public string NomActv { get; set; } = null!;
    public string CodSupr { get; set; } = null!;
    public string NomSupr { get; set; } = null!;
    public string RucSctta { get; set; } = null!;
    public string NomSctta { get; set; } = null!;
    public string CodCapataz { get; set; } = null!;
    public string NomCapataz { get; set; } = null!;
    public DateTime FechaFoc { get; set; }
    public string Coordenada { get; set; } = null!;
    public string Direccion { get; set; } = null!;
    public string Celular { get; set; } = null!;
    public string TpoTrabajo { get; set; } = null!;    
    public string? UsuarioApro { get; set; }
    public DateTime? FechaApro { get; set; }
    public string? Comentario { get; set; }
    public string? FolioRef { get; set; }
    public virtual Generico EstadoNav { get; set; } = null!;
    public virtual Generico FlujoNav { get; set; } = null!;
    public virtual Generico TipoNav { get; set; } = null!;
    public virtual ICollection<Servicio> Servicios { get; set; } = new List<Servicio>();
}
