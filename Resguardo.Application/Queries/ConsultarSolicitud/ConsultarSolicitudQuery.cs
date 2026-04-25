namespace Resguardo.Application.Queries.ConsultarSolicitud
{
    public class ConsultarSolicitudQuery
    {
        public string? Folio { get; set; }
        public int? IdTipo { get; set; }
        public int? IdEstado { get; set; }
        public int? IdFlujo { get; set; }
        public string? NumSro { get; set; }
    }
}