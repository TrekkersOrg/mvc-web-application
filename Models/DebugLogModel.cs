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

    [BsonElement("message")]
    public string? message { get; set; }


    public DebugLogModel()
    {
        timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        if (DateTime.TryParse(timestamp, out DateTime timestampDate))
        {
            expiresAfter = new BsonDateTime(timestampDate.AddDays(1));
        }  
    }
}