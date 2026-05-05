namespace ComprobantePago.Application.DTOs.Comprobante.Response
{
    public class DocumentoElectronicoDto
    {
        public int IdDocumento { get; set; }
        public string TipoArchivo { get; set; } = string.Empty;
        public string SubTipo { get; set; } = string.Empty;
        public string NombreArchivo { get; set; } = string.Empty;
        public string FechaReg { get; set; } = string.Empty;
        public long TamanioBytes { get; set; }
    }
}
