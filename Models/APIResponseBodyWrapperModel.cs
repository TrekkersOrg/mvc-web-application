namespace StriveAI.Models
{
    public class APIResponseBodyWrapperModel
    {
        public int StatusCode { get; set; }
        public string? StatusMessage { get; set; }
        public string? StatusMessageText { get; set;}
        public DateTime Timestamp { get; set; }
        public Object? Data { get; set; }
    }
}
