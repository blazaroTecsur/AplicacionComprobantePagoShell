namespace Reciclaje.Aplicacion.DTOs.Reciclaje
{
    // ── DTO de cada línea SRO mostrada en la grilla ──────────────────
    public class SroLineaValeDto
    {
        public int SrolineaId { get; set; }
        public string NumeroSro { get; set; } = null!;
        public int SroLineaSL { get; set; }
        public string? ArticuloNoInv { get; set; }
        public decimal? CantidadNoInv { get; set; }
        public string? UmnoInv { get; set; }
        public string? CodigoSupervisorNoInv { get; set; }
        public string? Ruc { get; set; }
        public string? DescripcionSubcontratista { get; set; }
        public string? OrdenCompra { get; set; }
        public string? EstadoSro { get; set; }
        public string? ArticuloReciclaje { get; set; }
        public string? DescripcionAlmacenNoInv { get; set; }
        public DateTime? FechaTransaccion { get; set; }
        public bool Seleccionado { get; set; }
        public string? UMReciclaje { get; set; }
        public string? EstadoLinea { get; set; }
    }

    // ── Filtros del buscador principal (Index) ───────────────────────
    public class ValeRecuperoBusquedaDto
    {
        public string? NumeroSro { get; set; }
        public DateTime? FechaTransaccion { get; set; }
        public string ArticuloNoInv { get; set; } = null!;
        public string? DescripcionArticulo { get; set; }
        public bool GenerarEspecifico { get; set; }
        public bool GenerarConsolidado { get; set; }
        public List<int> LineasSeleccionadas { get; set; } = new();
    }

    // ── ViewModel de la vista Index ──────────────────────────────────
    public class ValeRecuperoViewModel
    {
        public ValeRecuperoBusquedaDto Filtros { get; set; } = new();
        public List<SroLineaValeDto> Lineas { get; set; } = new();
        public bool BusquedaRealizada { get; set; }
    }
}
