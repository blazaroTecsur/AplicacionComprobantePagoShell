namespace Infor.Abstractions.DTOs
{
    public class IdoResponse
    {
        public string Bookmark { get; set; } = string.Empty;
        public string Message  { get; set; } = string.Empty;
        public int MessageCode { get; set; }
        public List<List<IdoItemsResponse>> Items { get; set; } = new();
        public bool MoreRowsExists { get; set; }
    }
}
