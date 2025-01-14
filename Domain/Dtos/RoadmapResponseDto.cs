namespace Application.DTOs
{
    public class RoadmapResponseDto
    {
        public Guid RoadmapId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Guid CreatedBy { get; set; }
        public float OverallProgress { get; set; }
        public float OverallDuration { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsDraft { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<MilestoneResponseDto> Milestones { get; set; }
    }

    public class MilestoneResponseDto
    {
        public Guid MilestoneId { get; set; }
        public Guid RoadmapId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public float MilestoneProgress { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsDeleted { get; set; }
        public List<SectionResponseDto> Sections { get; set; }
    }

    public class SectionResponseDto
    {
        public Guid SectionId { get; set; }
        public Guid MilestoneId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsDeleted { get; set; }
        public List<TaskResponseDto> Tasks { get; set; }
    }

    public class TaskResponseDto
    {
        public Guid TaskId { get; set; }
        public Guid SectionId { get; set; }
        public string Name { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsDeleted { get; set; }
    }
}
