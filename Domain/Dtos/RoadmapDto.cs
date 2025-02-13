
using System.ComponentModel.DataAnnotations;

namespace Domain.Dtos
{
    public class RoadmapDto
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public bool IsDraft { get; set; }

        [Required]
        public Guid CreatedBy { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int OverallDuration  { get; set; }

        public List<MilestoneDto> Milestones { get; set; }
    }

    public class MilestoneDto
    {
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Milestone Name must be between 1 and 50 characters.")]
        public string Name { get; set; }
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Milestone Description must be between 1 and 50 characters.")]
        public string Description { get; set; }
        public List<SectionDto> Sections { get; set; }
    }

    public class SectionDto
    {
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Section Name must be between 1 and 50 characters.")]
        public string Name { get; set; }
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Section Description must be between 1 and 50 characters.")]
        public string Description { get; set; }
        public List<TaskDto> Tasks { get; set; }
    }

    public class TaskDto
    {
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Task Name must be between 1 and 50 characters.")]
        public string Name { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
    }
}
