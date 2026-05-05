namespace ComprobantePago.Application.DTOs.Comprobante.Response
{
    public class ValidacionSunatDto
    {
        public bool Exito { get; set; }
        public string EstadoSunat { get; set; }
        public string CodigoEstado { get; set; }  // ← nuevo
        public string Motivo { get; set; }
        public string Folio { get; set; } = string.Empty; // ← nuevo
        public DatosXmlDto Datos { get; set; }
    }
}
