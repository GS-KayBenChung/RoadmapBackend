namespace Domain
{
    public class Roadmap
    {
        public Guid RoadmapId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public float OverallProgress { get; set; }
        public float OverallDuration { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsDraft { get; set; }

        public UserRoadmap CreatedByUser { get; set; }  // For Easier Navigation
        public ICollection<Milestone> Milestones { get; set; } = new List<Milestone>();
    }

}
