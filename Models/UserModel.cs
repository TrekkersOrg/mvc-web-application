using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Trekkers_AA.Models;

public class UserModel
{
    [BsonId]
    public ObjectId Id { get; set; }
    public string? firstName { get; set; }
    public string? lastName { get; set; }
    public string? email { get; set; }
    public string? password { get; set; }
    public string? birthDate { get; set; }
}

