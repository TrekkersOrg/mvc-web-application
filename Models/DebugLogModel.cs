using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Trekkers_AA.Models;

public class DebugLogModel
{
    [BsonId]
    [BsonElement("_id")]
    public ObjectId Id { get; set; }

    [BsonElement("expiresAfter")]
    public BsonDateTime? expiresAfter { get; set; }

    [BsonElement("timestamp")]
    public string? timestamp { get; set; }

    [BsonElement("logs")]
    public List<LogEntry>? logs { get; set; }

    public DebugLogModel()
    {
        if (!string.IsNullOrEmpty(timestamp))
        {
            if (!string.IsNullOrEmpty(timestamp))
            {
                if (DateTime.TryParse(timestamp, out DateTime timestampDate))
                {
                    expiresAfter = new BsonDateTime(timestampDate.AddDays(1));
                }
            }
        }
    }

}
public class LogEntry
{
    [BsonElement("logTimestamp")]
    public string? logTimestamp { get; set; }

    [BsonElement("message")]
    public string? message { get; set; }
}
