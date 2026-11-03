
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.EntityFrameworkCore;

namespace PruebaTecnica_MonoLegal.Models
{
    [Collection("users")]
    public class User
    {
        [BsonId]
        public ObjectId _id { get; set; }

        public required string userName { get; set; }

        public required string password { get; set; }

        public required bool isAdmin { get; set; }

        public DateTime createdAt { get; set; } = DateTime.UtcNow;
    }
}
