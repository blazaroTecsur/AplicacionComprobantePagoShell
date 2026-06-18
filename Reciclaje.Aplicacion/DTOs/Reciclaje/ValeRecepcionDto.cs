namespace Reciclaje.Aplicacion.DTOs.Reciclaje
{
    // ── Filtros del buscador de Vales (Recepcionar) ──────────────────
    public class ValeRecuperoBuscarDto
    {
        public string? NumeroSro { get; set; }
        public string? NumeroVale { get; set; }
        public string? CodigoArticuloReciclaje { get; set; }
        public string? DescripcionArticuloReciclaje { get; set; }
        public DateTime? FechaVale { get; set; }
    }

    // ── Fila de la grilla de Vales (Recepcionar) ─────────────────────
    public class ValeRecuperoDto
    {
        public int ValeId { get; set; }
        public string NumeroVale { get; set; } = null!;

        public string ArticuloNoInventariado { get; set; } = null!;
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
        public bool CheckRecepcion { get; set; }
        public string? Estado { get; set; }
        public DateTime? FechaCreacion { get; set; }
    }

    // ── ViewModel de la vista RecepcionarVale ────────────────────────
    public class ValeRecuperoListaViewModel
    {
        public ValeRecuperoBuscarDto Filtros { get; set; } = new();
        public List<ValeRecuperoDto> Vales { get; set; } = new();
        public bool BusquedaRealizada { get; set; }
    }

    // ── DTO para guardar recepción ───────────────────────────────────
    public class ValeRecepcionDto
    {
        public int ValeId { get; set; }
        public string? ArticuloReciclaje { get; set; }
        public decimal? CantidadRecibida { get; set; }
        public decimal? PesoRecibido { get; set; }
        public bool CheckRecepcion { get; set; }
    }
}
