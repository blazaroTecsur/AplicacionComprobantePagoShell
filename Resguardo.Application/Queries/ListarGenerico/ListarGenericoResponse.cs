namespace Resguardo.Application.Queries.ListarGenerico
{
    public class ListarGenericoResponse
    {
        public int Id { get; set; }
        public string Tipo { get; set; } = null!;
        public string Codigo { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
    }
}
