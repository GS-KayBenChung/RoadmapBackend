using Domain.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Serilog;

namespace Application.RoadmapActivities
{
    public class DashboardList
    {
        public class Query : IRequest<DashboardStatsDto>{}

        public class Handler : IRequestHandler<Query, DashboardStatsDto>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<DashboardStatsDto> Handle(Query request, CancellationToken cancellationToken)
            {
                var traceId = Guid.NewGuid().ToString();
                Log.Information("Fetching dashboard statistics...");

                var currentDate = DateTime.UtcNow;

                var roadmaps = await _context.Roadmaps
                    .Where(r => !r.IsDeleted)
                    .Select(r => new
                    {
                        r.IsCompleted,
                        r.IsDraft,
                        DueDate = r.Milestones
                            .SelectMany(m => m.Sections)
                            .SelectMany(s => s.ToDoTasks)
                            .Where(t => !t.IsDeleted)
                            .Max(t => (DateTime?)t.DateEnd) 
                    })
                    .ToListAsync(cancellationToken);

                int totalRoadmaps = roadmaps.Count;
                int completedRoadmaps = roadmaps.Count(r => r.IsCompleted);
                int draftRoadmaps = roadmaps.Count(r => r.IsDraft);
                int publishedRoadmaps = totalRoadmaps - draftRoadmaps; 
                int nearDueRoadmaps = roadmaps.Count(r => r.DueDate.HasValue && r.DueDate.Value > currentDate && r.DueDate.Value <= currentDate.AddDays(7) && !r.IsDraft);
                int overdueRoadmaps = roadmaps.Count(r => r.DueDate.HasValue && r.DueDate.Value < currentDate && !r.IsDraft);

                Log.Information("Dashboard statistics fetched successfully. Total: {TotalRoadmaps}, Completed: {CompletedRoadmaps}, Draft: {DraftRoadmaps}, Published: {PublishedRoadmaps}, Near Due: {NearDueRoadmaps}, Overdue: {OverdueRoadmaps}",
                    totalRoadmaps, completedRoadmaps, draftRoadmaps, publishedRoadmaps, nearDueRoadmaps, overdueRoadmaps);

                return new DashboardStatsDto
                {
                    TotalRoadmaps = totalRoadmaps,
                    CompletedRoadmaps = completedRoadmaps,
                    DraftRoadmaps = draftRoadmaps,
                    PublishedRoadmaps = publishedRoadmaps,
                    NearDueRoadmaps = nearDueRoadmaps,
                    OverdueRoadmaps = overdueRoadmaps
                };
            }
        }
    }
}
