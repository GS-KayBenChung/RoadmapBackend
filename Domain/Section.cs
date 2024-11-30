namespace Domain
{
    public class Section
    {
        public Guid SectionId { get; set; }
        public Guid MilestoneId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Milestone Milestone { get; set; }  // For Easier Navigation
        public ICollection<ToDoTask> ToDoTasks { get; set; } = new List<ToDoTask>();
    }
}
