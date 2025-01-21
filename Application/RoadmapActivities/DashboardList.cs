using Domain;
using Domain.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Serilog;

namespace Application.RoadmapActivities
{
    public class DashboardList
    {
        public class Query : IRequest<DashboardStatsDto> { }

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
                var currentDate = DateTime.UtcNow;

                Log.Information("[{Timestamp:yyyy-MM-dd HH:mm:ss}] [INFO] [TraceId: {TraceId}] Started processing DashboardList query",
                    DateTime.UtcNow, traceId);

                var roadmaps = await _context.Roadmaps
                    .Where(r => !r.IsDeleted)
                    .Include(r => r.Milestones)
                        .ThenInclude(m => m.Sections)
                            .ThenInclude(s => s.ToDoTasks)
                    .ToListAsync(cancellationToken);

                int totalRoadmaps = roadmaps.Count;
                int completedRoadmaps = 0;
                int nearDueRoadmaps = 0;
                int overdueRoadmaps = 0;
                int draftRoadmaps = 0;
                int publishedRoadmaps = 0;

                foreach (var roadmap in roadmaps)
                {
                    if (roadmap.IsCompleted)
                    {
                        completedRoadmaps++;
                    }

                    if (roadmap.IsDraft)
                    {
                        draftRoadmaps++;
                    }

                    if (!roadmap.IsDraft)
                    {
                        publishedRoadmaps++;
                    }

                    var dueDate = GetLatestTaskDueDate(roadmap);
                    if (dueDate != null)
                    {
                        if (dueDate.Value <= currentDate.AddDays(7) && dueDate.Value > currentDate)
                        {
                            if(!roadmap.IsDraft)
                                nearDueRoadmaps++;
                        }
                        if (dueDate.Value < currentDate)
                        {
                            if (!roadmap.IsDraft)
                                overdueRoadmaps++;
                        }
                    }
                }

                Log.Information("[{Timestamp:yyyy-MM-dd HH:mm:ss}] [INFO] [TraceId: {TraceId}] Completed processing DashboardList query. Total: {TotalRoadmaps}, Completed: {CompletedRoadmaps}, Draft: {DraftRoadmaps}, NearDue: {NearDueRoadmaps}, Overdue: {OverdueRoadmaps}",
                    DateTime.UtcNow, traceId, totalRoadmaps, completedRoadmaps, draftRoadmaps, nearDueRoadmaps, overdueRoadmaps);

                return new DashboardStatsDto
                {
                    TotalRoadmaps = totalRoadmaps,
                    CompletedRoadmaps = completedRoadmaps,
                    DraftRoadmaps = draftRoadmaps,
                    NearDueRoadmaps = nearDueRoadmaps,
                    OverdueRoadmaps = overdueRoadmaps,
                    PublishedRoadmaps = publishedRoadmaps
                };
            }

            private DateTime? GetLatestTaskDueDate(Roadmap roadmap)
            {
                DateTime? latestDate = null;

                foreach (var milestone in roadmap.Milestones)
                {
                    foreach (var section in milestone.Sections)
                    {
                        foreach (var task in section.ToDoTasks)
                        {
                            if (!task.IsDeleted)
                            {
                                var taskEndDate = task.DateEnd;
                                if (!latestDate.HasValue || taskEndDate > latestDate)
                                {
                                    latestDate = taskEndDate;
                                }
                            }
                        }
                    }
                }
                return latestDate;
            }
        }
    }
}