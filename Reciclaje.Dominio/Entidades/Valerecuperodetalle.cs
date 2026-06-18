namespace Reciclaje.Dominio.Entidades;

/// <summary>
/// Representa cada línea SROLinea seleccionada que compone un Vale de Recupero
/// (tanto Específico como Consolidado).
/// Tabla: valerecupero_detalle
/// </summary>
public class Valerecuperodetalle
{
    public int DetalleId { get; set; }

    /// <summary>FK → valerecupero.ValeID</summary>
    public int ValeId { get; set; }

    /// <summary>FK → srolinea.SROLineaID</summary>
    public int SrolineaId { get; set; }

    /// <summary>FK → sro.SROID</summary>
    public int SroId { get; set; }

    /// <summary>Número de línea en SyteLine.</summary>
    public int SroLineaSL { get; set; }

    /// <summary>Artículo de no-inventario (copia de Srolinea.ArticuloNoInv).</summary>
    public string? ArticuloNoInv { get; set; }

    /// <summary>Cantidad original de la línea SRO.</summary>
    public decimal? CantidadNoInv { get; set; }

    /// <summary>Unidad de medida no-inventario.</summary>
    public string? UmnoInv { get; set; }

    public DateTime? FechaTransaccion { get; set; }

    public string? CodigoAlmacenNoInv { get; set; }

    public string? DescripcionAlmacenNoInv { get; set; }

    public string? CodigoSupervisorNoInv { get; set; }

    public string? DescripcionSupervisorNoInv { get; set; }

    /// <summary>Artículo reciclaje (copia de Srolinea.ArticuloReciclaje).</summary>
    public string? ArticuloReciclaje { get; set; }

    /// <summary>Unidad de medida reciclaje.</summary>
    public string? UMReciclaje { get; set; }

    public string? OrdenCompra { get; set; }

    public string? EstadoLinea { get; set; }

    public DateTime? FechaCreacionAudit { get; set; }

    public string? UsuarioCreacionAudit { get; set; }

    public DateTime? FechaModificacionAudit { get; set; }

    public string? UsuarioModificacionAudit { get; set; }

    public decimal? CantidadRecibida { get; set; }

    public decimal? PesoRecibido { get; set; }

    public bool? CheckRecepcion { get; set; }

    /// <summary>GUID único por fila (UNIQUE en BD).</summary>
    public string RowPointer { get; set; } = Guid.NewGuid().ToString();

    // ── Navegación ──────────────────────────────────────────────────────────
    public virtual Valerecupero Valerecupero { get; set; } = null!;
    public virtual Srolinea     Srolinea     { get; set; } = null!;
}
