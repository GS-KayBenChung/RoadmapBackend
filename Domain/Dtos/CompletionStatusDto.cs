

namespace Domain.Dtos
    {
    public class CompletionStatusDto
    {
        public Guid Id { get; set; }
        public bool? IsCompleted { get; set; }
        public int? Progress { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

