namespace Domain.Dtos
{
    public class RoadmapLogsDto
    {
        public Guid LogId { get; set; }
        public Guid UserId { get; set; }
        public string ActivityAction { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserName { get; set; } 
    }

}
