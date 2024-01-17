namespace StriveAI.Models
{
    public class PurgePineconeResponseModel
    {
        public int NumberOfVectorsDeleted { get; set; }
        public string? Namespace { get; set; }
    }
}
