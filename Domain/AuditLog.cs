namespace Domain
{
    public class AuditLog
    {
        public Guid LogId { get; set; }
        public Guid UserId { get; set; }
        public string ActivityAction { get; set; }
        public DateTime CreatedAt { get; set; }

        public UserRoadmap User { get; set; }  // For Easier Navigation
    }
}
