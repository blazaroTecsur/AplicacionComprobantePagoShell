namespace ComprobantePago.Application.DTOs.Comprobante.Requests
{
    public class ImputacionDto
    {
        public string Secuencia { get; set; } = string.Empty;
        public string Folio { get; set; } = string.Empty;
        public string AliasCuenta { get; set; } = string.Empty;
        public string CuentaContable { get; set; } = string.Empty;
        public string DescripcionCuenta { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string Proyecto { get; set; } = string.Empty;
        public string CodUnidad1Cuenta { get; set; } = string.Empty;
        public string CodUnidad3Cuenta { get; set; } = string.Empty;
        public string CodUnidad4Cuenta { get; set; } = string.Empty;
    }
}
