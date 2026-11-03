
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.EntityFrameworkCore;

namespace PruebaTecnica_MonoLegal.Models
{
    [Collection("notifications")]
    public class Notifications
    {
        [BsonId]
        public ObjectId _id { get; set; }

        public ObjectId invoiceId { get; set; }

        public ObjectId clientId { get; set; }

        public required string type { get; set; }

        public required string recipient { get; set; }
        public required int attemps { get; set; }


        public DateTime? createAt { get; set; } = DateTime.UtcNow;

        public DateTime? acceptedAt { get; set; }
        public required string targetStatus { get; set; }


        public required string result { get; set; }

        public string? error { get; set; }
    }
}
