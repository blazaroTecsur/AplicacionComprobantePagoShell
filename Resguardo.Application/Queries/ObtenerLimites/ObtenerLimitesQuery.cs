namespace Resguardo.Application.Queries.ObtenerConfig
{
    public class ObtenerLimitesQuery
    {
        public string CodDpto { get; set; } = null!;
        public DateOnly Fecha { get; set; }
    }
}
