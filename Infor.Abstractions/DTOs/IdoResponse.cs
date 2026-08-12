using System.Text.Json;

namespace Infor.Abstractions.DTOs
{
    public class IdoResponse
    {
        public bool   Success        { get; set; }
        public string Message        { get; set; } = string.Empty;
        public string Bookmark       { get; set; } = string.Empty;
        public bool   MoreRowsExist  { get; set; }
        public List<JsonElement> Items { get; set; } = new();
    }
}