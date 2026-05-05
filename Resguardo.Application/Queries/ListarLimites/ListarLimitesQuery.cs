namespace Resguardo.Application.Queries.ListarLimites
{
    public class ListarLimitesQuery
    {
        public DateOnly Fecha { get; set; }
        public string CodDpto { get; set; } = null!;
    }
}