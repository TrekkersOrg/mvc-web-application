using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Trekkers_AA.Models;

public class SessionModel
{
    [BsonId]
    public ObjectId Id { get; set; }
    public string? email { get; set; }
    public string? timestamp { get; set; }
    public string? file { get; set; }
    public bool status { get; set; }
}