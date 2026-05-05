namespace ComprobantePago.Application.DTOs.Comprobante.Common
{
    public class ComboDto
    {
        public string Codigo { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public decimal Porcentaje { get; set; }
    }
}
