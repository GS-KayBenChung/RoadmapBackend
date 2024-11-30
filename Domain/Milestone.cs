namespace Domain
{
    public class Milestone
    {
        public Guid MilestoneId { get; set; }
        public Guid RoadmapId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public float MilestoneProgress { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Roadmap Roadmap { get; set; }  // For Easier Navigation
        public ICollection<Section> Sections { get; set; } = new List<Section>();
    }

}
