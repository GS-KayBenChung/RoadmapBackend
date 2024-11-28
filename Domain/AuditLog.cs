namespace Domain
{
    public class AuditLog
    {
        public int LogId { get; set; }
        public int UserId { get; set; }
        public string ActivityAction { get; set; }
        public DateTime CreatedAt { get; set; }

        public UserRoadmap User { get; set; }  // For Easier Navigation
    }
}
