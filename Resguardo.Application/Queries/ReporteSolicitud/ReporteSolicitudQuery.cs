namespace Resguardo.Application.Queries.ReporteSolicitud
{
    public class ReporteSolicitudQuery
    {
        public DateOnly FechaDsd { get; set; }
        public DateOnly FechaHst { get; set; }
        public int? IdFlujo { get; set; }
    }
}