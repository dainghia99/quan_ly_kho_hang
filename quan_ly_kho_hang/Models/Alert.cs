using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace quan_ly_kho_hang.Models
{
    public class Alert
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string? ProductId { get; set; }
        public string Message { get; set; }
        public bool IsHandled { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
