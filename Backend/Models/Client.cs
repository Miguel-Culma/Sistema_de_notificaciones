using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.EntityFrameworkCore;

namespace PruebaTecnica_MonoLegal.Models
{
    [Collection("clients")]
    public class Client
    {
        [BsonId]
        public ObjectId _id { get; set; }

        public required string name { get; set; }

        public required string mail { get; set; }

        public bool active { get; set; } = true;

        public DateTime createdAt { get; set; } = DateTime.UtcNow;
    }
}
