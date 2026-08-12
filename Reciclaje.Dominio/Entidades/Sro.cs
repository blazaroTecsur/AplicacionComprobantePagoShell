namespace Reciclaje.Dominio.Entidades;

public partial class Sro 
{
    public int Sroid { get; set; }

    public string NumeroSro { get; set; } = null!;

    public string? CodigoSubcontratista { get; set; }

    public string? DescripcionSubcontratista { get; set; }

    public string? Ruc { get; set; }

    public string? Sitio { get; set; }

    public string? Estado { get; set; }

    public DateTime? FechaCreacionAudit { get; set; }

    public string? UsuarioCreacionAudit { get; set; }

    public DateTime? FechaModificacionAudit { get; set; }

    public string? UsuarioModificacionAudit { get; set; }

    public virtual ICollection<Srolinea> Srolineas { get; set; } = new List<Srolinea>();
}
