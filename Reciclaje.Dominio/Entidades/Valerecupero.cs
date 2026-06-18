namespace Reciclaje.Dominio.Entidades;

public partial class Valerecupero
{
    public int ValeId { get; set; }

    public string NumeroVale { get; set; } = null!;

    public string? TipoVale { get; set; }

    public int SrolineaId { get; set; }

    public string ArticuloReciclaje { get; set; } = null!;

    public int? ArticuloReciclajeId { get; set; }

    public decimal? CantidadRecibida { get; set; }

    public decimal? PesoRecibido { get; set; }

    public string? Umreciclaje { get; set; }

    public string? Ocanual { get; set; }

    public DateTime? FechaRecepcion { get; set; }

    public bool? CheckRecepcion { get; set; }

    public int? UsuarioRecepcionId { get; set; }

    public DateTime? FechaConfirmacion { get; set; }

    public bool? CheckConfirmacion { get; set; }

    public int? UsuarioConfirmacionId { get; set; }

    public string? Estado { get; set; }

    public DateTime? FechaCreacionAudit { get; set; }

    public string? UsuarioCreacionAudit { get; set; }

    public DateTime? FechaModificacionAudit { get; set; }

    public string? UsuarioModificacionAudit { get; set; }

    public virtual Srolinea Srolinea { get; set; } = null!;

    public decimal? CantidadReciclaje { get; set; }

    public decimal? CostoUnitario { get; set; }

}
