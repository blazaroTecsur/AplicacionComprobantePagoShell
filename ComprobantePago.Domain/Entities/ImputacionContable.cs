namespace ComprobantePago.Domain.Entities
{
    public class ImputacionContable
    {
        public int IdImputacionContable { get; set; }
        public string Folio { get; set; } = string.Empty;
        public int Secuencia { get; set; }
        public string? AliasCuenta { get; set; }
        public string? CuentaContable { get; set; }
        public string? DescripcionCuenta { get; set; }
        public decimal Monto { get; set; }
        public string? Descripcion { get; set; }
        public string? Proyecto { get; set; }
        public string? CodUnidad1Cuenta { get; set; }
        public string? CodUnidad3Cuenta { get; set; }
        public string? CodUnidad4Cuenta { get; set; }
        public string UsuarioReg { get; set; } = string.Empty;
        public DateTime FechaReg { get; set; } = DateTime.Now;
        public string? UsuarioAct { get; set; }
        public DateTime? FechaAct { get; set; }

        // Navegación
        public Comprobante Comprobante { get; set; } = null!;
    }
}
