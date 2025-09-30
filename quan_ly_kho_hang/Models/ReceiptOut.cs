using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace quan_ly_kho_hang.Models
{
    public class ReceiptOutItem
    {
        [BsonRepresentation(BsonType.ObjectId)]
        [Required]
        public string ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int? Quantity { get; set; }

        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }
    }

    public class ReceiptOut
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [Required]
        [StringLength(200)]
        public string ReceiptNumber { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public List<ReceiptOutItem> Items { get; set; } = new();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? CreatedByUserEmail { get; set; }
    }
}
