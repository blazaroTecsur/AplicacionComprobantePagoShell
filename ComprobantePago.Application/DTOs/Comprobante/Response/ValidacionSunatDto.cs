namespace ComprobantePago.Application.DTOs.Comprobante.Response
{
    public class ValidacionSunatDto
    {
        public bool Exito { get; set; }
        public string EstadoSunat  { get; set; } = string.Empty;
        public string CodigoEstado { get; set; } = string.Empty;
        public string Motivo       { get; set; } = string.Empty;
        public string Folio        { get; set; } = string.Empty;
        public DatosXmlDto? Datos  { get; set; }
    }
}
