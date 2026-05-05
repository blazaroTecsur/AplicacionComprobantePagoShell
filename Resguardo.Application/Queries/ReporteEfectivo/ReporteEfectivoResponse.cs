namespace Resguardo.Application.Queries.ReporteEfectivo
{
    public class ReporteEfectivoResponse
    {
        public DateOnly Fecha { get; set; }
        public string Nombres { get; set; } = null!;
        public string? Telefono { get; set; }
        public string Direccion { get; set; } = null!;
        public string Coordenada { get; set; } = null!;
        public string TipoServicio { get; set; } = null!;
        public string HraInicio { get; set; } = null!;
        public string HraFinal { get; set; } = null!;
        public string? HraInicioReal { get; set; }
        public string? HraFinalReal { get; set; }
        public string? HraAmplia { get; set; } = null!;
        public string UsuarioReg { get; set; } = null!;
        public string CodDpto { get; set; } = null!;
        public string NomDpto { get; set; } = null!;
        public string NomActv { get; set; } = null!;
        public string NomSctta { get; set; } = null!;
        public string NomCapataz { get; set; } = null!;
        public string Celular { get; set; } = null!;
        public string NumSro { get; set; } = null!;
        public string Folio { get; set; } = null!;
    }
}
