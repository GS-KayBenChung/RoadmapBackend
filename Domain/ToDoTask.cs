namespace Domain
{
    public class ToDoTask
    {
        public int TaskId { get; set; }
        public int SectionId { get; set; }
        public string Name { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Section Section { get; set; } // For Easier Navigation
    }
}