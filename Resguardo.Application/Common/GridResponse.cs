namespace Resguardo.Application.Common
{
    public class GridResponse<T>
    {
        public List<T> Data { get; set; }
        public int Total { get; set; }
    }
}
