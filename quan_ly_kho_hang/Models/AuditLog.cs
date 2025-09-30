using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace quan_ly_kho_hang.Models
{
    public class AuditLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string Action { get; set; } // e.g. "InventoryApplied"
        public string? PerformedByUserId { get; set; }
        public string? PerformedByEmail { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Details { get; set; } // JSON or text description
    }
}
