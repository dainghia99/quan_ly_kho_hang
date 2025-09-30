using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace quan_ly_kho_hang.Models
{
    public enum InventoryStatus
    {
        Draft,
        Completed,
        Applied,
        Canceled
    }

    public class InventoryItem
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string? ProductId { get; set; }

        public string? ProductName { get; set; } // snapshot
        public int ExpectedQuantity { get; set; } // tồn kho hệ thống
        public int CountedQuantity { get; set; } // nhập khi kiểm kê
        public int Difference => CountedQuantity - ExpectedQuantity;
        public string? Note { get; set; }
    }

    public class Inventory
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [Required]
        [StringLength(200)]
        public string InventoryNumber { get; set; } = $"INV-{DateTime.UtcNow:yyyyMMddHHmmss}";

        public List<InventoryItem> Items { get; set; } = new();

        public InventoryStatus Status { get; set; } = InventoryStatus.Draft;

        public string? CreatedByUserId { get; set; }
        public string? CreatedByUserEmail { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public DateTime? AppliedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public string? Notes { get; set; }
    }
}
