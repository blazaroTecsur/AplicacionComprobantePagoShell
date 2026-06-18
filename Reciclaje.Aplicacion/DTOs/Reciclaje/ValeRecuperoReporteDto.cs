namespace Reciclaje.Aplicacion.DTOs.Reciclaje
{
    /// <summary>
    /// Fila del reporte PDF leída desde valerecupero_detalle.
    /// Cada fila representa un detalle individual (línea por línea).
    /// </summary>
    public class ValeRecuperoReporteFilaDto
    {
        public string NumeroSro { get; set; } = string.Empty;
        public string CodigoSST { get; set; } = string.Empty;  // CodigoSupervisorNoInv
        public string DescripcionSST { get; set; } = string.Empty;  // DescripcionSupervisorNoInv
        public string ArticuloNoInventariado { get; set; } = string.Empty;
        public string UmNoInv { get; set; } = string.Empty;
        public string ArticuloReciclaje { get; set; } = string.Empty;
        public string DescripcionArticuloReciclaje { get; set; } = string.Empty;
        public string UnidadMedidaReciclaje { get; set; } = string.Empty;
        public string NroVale { get; set; } = string.Empty;
        public decimal? CantidadNoInv { get; set; }
        public decimal? CantidadRecibida { get; set; }
        public decimal? Peso { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string Ruc { get; set; } = string.Empty;
        public string Dept { get; set; } = string.Empty;
    }

    /// <summary>
    /// Grupo de filas que pertenecen a un mismo NumeroSRO.
    /// Cada grupo genera su propia sección en el PDF con todas
    /// las líneas de valerecupero_detalle de ese SRO.
    /// </summary>
    public class ValeRecuperoReporteGrupoDto
    {
        public string NumeroSro { get; set; } = string.Empty;

        public List<ValeRecuperoReporteFilaDto> Filas { get; set; } = new();

        public decimal TotalCantidadRecibida => Filas.Sum(f => f.CantidadRecibida ?? 0m);
        public decimal TotalPeso => Filas.Sum(f => f.Peso ?? 0m);
    }

    /// <summary>
    /// DTO raíz del reporte: grupos ordenados por NumeroSRO.
    /// </summary>
    public class ValeRecuperoReporteDto
    {
        public ValeRecuperoBuscarDto Filtros { get; set; } = new();
        public List<ValeRecuperoReporteGrupoDto> Grupos { get; set; } = new();
        public DateTime FechaGeneracion { get; set; } = DateTime.Now;

        public decimal TotalCantidadRecibida => Grupos.Sum(g => g.TotalCantidadRecibida);
        public decimal TotalPeso => Grupos.Sum(g => g.TotalPeso);
        public int TotalRegistros => Grupos.Sum(g => g.Filas.Count);
    }
}
