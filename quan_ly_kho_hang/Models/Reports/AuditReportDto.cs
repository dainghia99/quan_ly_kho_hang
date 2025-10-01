namespace quan_ly_kho_hang.Models.Reports
{
    public class AuditLogDto
    {
        public string Id { get; set; } = null!;
        public string Action { get; set; } = null!;
        public string? PerformedByEmail { get; set; }
        public DateTime Timestamp { get; set; }
        public string Details { get; set; } = null!;
    }
}
