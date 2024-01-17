using System.Text.Json;

namespace StriveAI.Models
{
    public class PurgePineconeRequestModel
    {
        public string? Namespace { get; set; }

        public bool DeleteAll { get; set; } = true;

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
