namespace Resguardo.Application.Common
{
    public class GridRequest<T>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public T? Filtros { get; set; }
    }
}
