namespace Resguardo.Application.Queries.ObtenerSolicitud
{
    public class ObtenerSolicitudResponse
    {
        public int Id { get; set; }
        public string Folio { get; set; } = null!;
        public int IdEstado { get; set; }
        public int IdFlujo { get; set; }
        public string NumSro { get; set; } = null!;
        public string CodDpto { get; set; } = null!;
        public string NomDpto { get; set; } = null!;
        public string CodActv { get; set; } = null!;
        public string NomActv { get; set; } = null!;
        public string RucSctta { get; set; } = null!;
        public string NomSctta { get; set; } = null!;
        public string CodCapataz { get; set; } = null!;
        public string NomCapataz { get; set; } = null!;
        public string Celular { get; set; } = null!;
        public string TpoTrabajo { get; set; } = null!;
        public string Coordenada { get; set; } = null!;
        public string Direccion { get; set; } = null!;
        public string? HrasSolicitadas { get; set; }
        public string? HrasAprobadas { get; set; }
        public virtual ICollection<ObtenerServicioResponse> Servicios { get; set; } = new List<ObtenerServicioResponse>();
    }
}
