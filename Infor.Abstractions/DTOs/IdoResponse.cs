namespace Infor.Abstractions.DTOs
{
    public class IdoResponse
    {
        public string Bookmark { get; set; }
        public string Message { get; set; }
        public int MessageCode { get; set; }
        public List<List<IdoItemsResponse>> Items { get; set; }
        public bool MoreRowsExists { get; set; }
    }
}
