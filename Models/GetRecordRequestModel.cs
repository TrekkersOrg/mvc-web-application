using System.Text.Json;

namespace StriveAI.Models
{
    public class GetRecordRequestModel
    {
        public List<string>? Ids { get; set; }
        public string? Namespace { get; set; }

        public string ToJson()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });
        }
    }
}
