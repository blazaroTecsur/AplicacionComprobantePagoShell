namespace Resguardo.Application.Queries.ReporteSolicitud
{
    public class ReporteSolicitudResponse
    {
        public DateTime FechaReg { get; set; }
        public string Flujo { get; set; } = null!;
        public string Folio { get; set; } = null!;
        public string NumSro { get; set; } = null!;
        public string Estado { get; set; } = null!;
        public string RucSctta { get; set; } = null!;
        public string NomSctta { get; set; } = null!;
        public string NomSupr { get; set; } = null!;
        public string NomCapataz { get; set; } = null!;
        public string Celular { get; set; } = null!;
        public string TpoTrabajo { get; set; } = null!;
        public string CodDpto { get; set; } = null!;
        public string NomDpto { get; set; } = null!;
        public string? UsuarioApro { get; set; }
        public DateTime? FechaApro { get; set; } 
        public string TipoServicio { get; set; } = null!;
        public DateOnly Fecha { get; set; }
        public string HraInicio { get; set; } = null!;
        public string HraFinal { get; set; } = null!;
        public string Turno { get; set; } = null!;
        public int Cantidad { get; set; }
        public string Direccion { get; set; } = null!;
        public string Coordenada { get; set; } = null!;
        public string? Comentario { get; set; }
    }
}
