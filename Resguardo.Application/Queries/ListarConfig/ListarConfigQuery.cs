namespace Resguardo.Application.Queries.ListarConfig
{
    public class ListarConfigQuery
    {
        public DateOnly Fecha { get; set; }
        public string CodDpto { get; set; } = null!;
    }
}