using Domain;

namespace Application.DTOs
{
    public class RoadmapDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsDraft { get; set; }
        public Guid CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<MilestoneDto> Milestones { get; set; }
    }

    public class MilestoneDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<SectionDto> Sections { get; set; }
    }

    public class SectionDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<TaskDto> Tasks { get; set; }
    }

    public class TaskDto
    {
        public string Name { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
    }
}
