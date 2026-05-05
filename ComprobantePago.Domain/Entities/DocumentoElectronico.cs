namespace ComprobantePago.Domain.Entities
{
    public class DocumentoElectronico
    {
        public int IdDocumento { get; set; }
        public string Folio { get; set; } = string.Empty;
        public string TipoArchivo { get; set; } = string.Empty;  // XML, PDF, MSG, EML
        public string SubTipo { get; set; } = string.Empty;      // XML_SUNAT, XML_CDR, REPRESENTACION_IMPRESA, etc.
        public string NombreArchivo { get; set; } = string.Empty;
        public byte[] Contenido { get; set; } = Array.Empty<byte>();
        public DateTime FechaReg { get; set; }
        public string UsuarioReg { get; set; } = string.Empty;
    }
}
