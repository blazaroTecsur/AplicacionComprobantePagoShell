namespace ComprobantePago.Domain.Entities
{
    public class SerieCorrelativo
    {
        public int IdSerieCorrelativo { get; set; }
        public string CodigoEmpresa { get; set; } = string.Empty;
        public string TipoDocumento { get; set; } = string.Empty; // PV, VC, PT
        public int Anio { get; set; }
        public int Mes { get; set; }
        public int UltimoCorrelativo { get; set; }
    }
}
