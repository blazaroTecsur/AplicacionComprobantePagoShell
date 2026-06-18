namespace Reciclaje.Aplicacion.DTOs.Reciclaje
{
    // ── Fila de la grilla de Confirmación ───────────────────────────
    public class ValeConfirmacionDto
    {
        public int ValeId { get; set; }
        public string NumeroVale { get; set; } = null!;
        public string? TipoVale { get; set; }
        public string NumeroSro { get; set; } = null!;
        public string? Contratista { get; set; }
        public string? OrdenCompra { get; set; }
        public string? UmReciclaje { get; set; }
        public decimal? CantidadReciclaje { get; set; }
        public string? OcAnual { get; set; }
        public string? CodigoArticuloReciclaje { get; set; }
        public decimal? CantidadReal { get; set; }
        public decimal? Peso { get; set; }
        public decimal? CantidadPendiente { get; set; }
        public bool CheckConfirmacion { get; set; }
        public DateTime? FechaConfirmacion { get; set; }
        public string? Estado { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public string? RowPointer { get; set; }

        // ── Campos adicionales para el reporte Excel ─────────────────
        public string? ArticuloNoInventariado { get; set; }
        public string? UmNoInv { get; set; }
        public string? DescripcionArticuloReciclaje { get; set; }
        public DateTime? FechaRecepcion { get; set; }
        public string? Ruc { get; set; }

        /// <summary>Suma de CantidadNoInv de valerecupero_detalle para este vale.</summary>
        public decimal? CantidadNoInv { get; set; }
        public string? Dept { get; set; }
    }

    // ── ViewModel de la vista ConfirmarVale ──────────────────────────
    public class ValeConfirmacionListaViewModel
    {
        public ValeRecuperoBuscarDto Filtros { get; set; } = new();
        public List<ValeConfirmacionDto> Vales { get; set; } = new();
        public bool BusquedaRealizada { get; set; }
    }

    // ── DTO para guardar / rechazar confirmación ─────────────────────
    public class ValeConfirmacionGuardarDto
    {
        public int ValeId { get; set; }
        public string? ArticuloReciclaje { get; set; }
        public decimal? CantidadRecibida { get; set; }
        public decimal? PesoRecibido { get; set; }
        public bool CheckConfirmacion { get; set; }
        public bool Seleccionado { get; set; }
    }
}
