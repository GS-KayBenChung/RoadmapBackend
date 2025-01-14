namespace Domain.Dtos
{
    public class DashboardStatsDto
    {
        public int TotalRoadmaps { get; set; }
        public int CompletedRoadmaps { get; set; }
        public int NearDueRoadmaps { get; set; }
        public int OverdueRoadmaps { get; set; }
        public int DraftRoadmaps { get; set; }
        public int PublishedRoadmaps { get; set; }
    }
}
