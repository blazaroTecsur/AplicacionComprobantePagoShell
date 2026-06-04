namespace Infor.Abstractions.DTOs
{
    public class IdoResponse
    {
        public bool   Success        { get; set; }
        public string Message        { get; set; } = string.Empty;
        public string Bookmark       { get; set; } = string.Empty;
        public bool   MoreRowsExist  { get; set; }
        public List<List<IdoItemsResponse>> Items { get; set; } = new();
    }
}
