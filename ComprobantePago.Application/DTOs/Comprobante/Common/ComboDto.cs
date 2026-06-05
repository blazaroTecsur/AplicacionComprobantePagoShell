namespace ComprobantePago.Application.DTOs.Comprobante.Common
{
    public class ComboDto
    {
        public string Codigo      { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal Porcentaje { get; set; }
    }
}
