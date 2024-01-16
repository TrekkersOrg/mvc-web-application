namespace StriveAI.Models
{
    public class PineconeDetailsResponseModel
    {
        public Dictionary<string, NamespaceModel>? Namespaces { get; set; }
        public int Dimension { get; set; }
        public double IndexFullness { get; set; }
        public int TotalVectorCount { get; set; }
        public class NamespaceModel
        {
            public int VectorCount { get; set; }
        }
    }
}
