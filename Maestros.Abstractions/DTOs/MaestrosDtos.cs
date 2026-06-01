namespace Maestros.Abstractions.DTOs
{
    public class ApiResponse<T>
    {
        public bool   Exito   { get; set; }
        public T?     Data    { get; set; }
        public string Mensaje { get; set; } = string.Empty;
    }

    public class PagedResult<T>
    {
        public IEnumerable<T> Items  { get; set; } = [];
        public int            Total  { get; set; }
        public int            Pagina { get; set; }
        public int            Tamano { get; set; }
    }
}
