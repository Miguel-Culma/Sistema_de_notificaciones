using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.EntityFrameworkCore;

namespace PruebaTecnica_MonoLegal.Models
{
    [Collection("reminderSettings")]
    public class ReminderSettings
    {
        [BsonId]
        public ObjectId _id { get; set; }

        public int intervalMinutes { get; set; } = 5;

        public bool enabled { get; set; } = true;

        public DateTime updatedAt { get; set; } = DateTime.UtcNow;
    }
}