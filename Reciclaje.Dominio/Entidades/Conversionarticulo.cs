namespace Reciclaje.Dominio.Entidades;

public partial class Conversionarticulo 
{
    public int ConversionId { get; set; }

    public string ArticuloNoInventariado { get; set; } = null!;

    public string ArticuloReciclaje { get; set; } = null!;

    public string DescripcionArticuloReciclaje { get; set; }

    public DateTime? FechaCreacionAudit { get; set; }

    public string? UsuarioCreacionAudit { get; set; }

    public DateTime? FechaModificacionAudit { get; set; }

    public string? UsuarioModificacionAudit { get; set; }

    public virtual ICollection<Srolinea> Srolineas { get; set; }

}
