
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.EntityFrameworkCore;

namespace PruebaTecnica_MonoLegal.Models
{
    [Collection("invoices")]
    public class Invoice
    {
        [BsonId]
        public ObjectId _id { get; set; }

        public required string invoiceNumber { get; set; }

        public ObjectId clientId { get; set; }

        public decimal amount { get; set; }

        public required string currency { get; set; }

        public DateTime createAt { get; set; }

        public DateTime dueDate { get; set; }

        public required string status { get; set; }

        public DateTime updatedAt { get; set; } = DateTime.UtcNow;
    }
}
