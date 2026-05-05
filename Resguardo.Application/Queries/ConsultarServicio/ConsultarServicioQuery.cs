namespace Resguardo.Application.Queries.ConsultarServicio
{
    public class ConsultarServicioQuery
    {
        public int? IdProveedor { get; set; }
        public int? IdTpoServicio { get; set; }
        public string? Folio { get; set; }
        public DateOnly? FechaIni { get; set; }
        public DateOnly? FechaFin { get; set; }
    }
}
