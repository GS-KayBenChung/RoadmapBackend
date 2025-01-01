using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.RoadmapActivities
{
    public class DashboardList
    {
        public class Query : IRequest<DashboardStats> { }

        public class Handler : IRequestHandler<Query, DashboardStats>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<DashboardStats> Handle(Query request, CancellationToken cancellationToken)
            {
                var currentDate = DateTime.UtcNow;

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

                    var dueDate = GetLatestTaskDueDate(roadmap);
                    if (dueDate != null)
                    {
                        if (dueDate.Value <= currentDate.AddDays(7) && dueDate.Value > currentDate)
                        {
                            nearDueRoadmaps++;
                        }
                        if (dueDate.Value < currentDate)
                        {
                            overdueRoadmaps++;
                        }
                    }
                }

                return new DashboardStats
                {
                    TotalRoadmaps = totalRoadmaps,
                    CompletedRoadmaps = completedRoadmaps,
                    DraftRoadmaps = draftRoadmaps,
                    NearDueRoadmaps = nearDueRoadmaps,
                    OverdueRoadmaps = overdueRoadmaps
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
