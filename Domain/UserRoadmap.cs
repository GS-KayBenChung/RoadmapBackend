namespace Domain
{
    public class UserRoadmap
    {
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string GoogleId { get; set; }

        public DateTime CreatedAt { get; set; }

        public ICollection<Roadmap> Roadmaps { get; set; } = new List<Roadmap>();
        public ICollection<AuditLog> Logs { get; set; } = new List<AuditLog>();
    }

}
