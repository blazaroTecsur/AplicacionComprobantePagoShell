namespace Reciclaje.Dominio.Entidades;

public partial class Srolinea 
{
    public int SrolineaId { get; set; }

    public int Sroid { get; set; }

    public int SroLineaSL { get; set; }

    public string ArticuloNoInv { get; set; } = null!;

    public decimal? CantidadNoInv { get; set; }

    public string? UmnoInv { get; set; }

    public DateTime? FechaTransaccion { get; set; }

    public string? CodigoAlmacenNoInv { get; set; }

    public string? DescripcionAlmacenNoInv { get; set; }

    public string? CodigoSupervisorNoInv { get; set; }

    public string? DescripcionSupervisorNoInv { get; set; }

    public string? ArticuloReciclaje { get; set; }

    public string? OrdenCompra { get; set; }

    public string? EstadoLinea { get; set; }

    public DateTime? FechaCreacionAudit { get; set; }

    public string? UsuarioCreacionAudit { get; set; }

    public DateTime? FechaModificacionAudit { get; set; }

    public string? UsuarioModificacionAudit { get; set; }

    public virtual Sro Sro { get; set; } = null!;

    public string? UMReciclaje { get; set; }

    public string? RowPointer { get; set; }

    public string? TramaSyteLine { get; set; }

    public string? Dept { get; set; }

    public virtual ICollection<Valerecupero> Valerecuperos { get; set; } = new List<Valerecupero>();

    public int? ConversionID { get; set; }  // Nullable
    public virtual Conversionarticulo Conversionarticulo { get; set; }

}
