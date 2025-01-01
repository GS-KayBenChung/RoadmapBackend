using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.RoadmapActivities
{
    public class List
    {
        public class Query : IRequest<PaginatedRoadmapResult<Roadmap>>
        {
            public string Filter { get; set; }
            public string Search { get; set; }
            public DateTime? CreatedAfter { get; set; }
            public int PageNumber { get; set; } = 1;
            public int PageSize { get; set; } = 10;
        }
        public class Handler : IRequestHandler<Query, PaginatedRoadmapResult<Roadmap>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<PaginatedRoadmapResult<Roadmap>> Handle(Query request, CancellationToken cancellationToken)
            {
                Console.WriteLine($"Handling request for filter: {request.Filter}, created after: {request.CreatedAfter}");

                var query = _context.Roadmaps
                    .Where(r => !r.IsDeleted)
                    .OrderByDescending(r => r.UpdatedAt)
                    .AsNoTracking()
                    .AsQueryable();

                if (request.CreatedAfter.HasValue)
                {
                    var startOfDay = request.CreatedAfter.Value.Date;
                    var endOfDay = startOfDay.AddDays(1).AddTicks(-1);
                    query = query.Where(r => r.CreatedAt >= startOfDay && r.CreatedAt <= endOfDay);
                    Console.WriteLine($"Filtering created after: {startOfDay} to {endOfDay}");
                }

                if (!string.IsNullOrEmpty(request.Filter))
                {
                    Console.WriteLine($"Applying filter: {request.Filter.ToLower()}");
                    if (request.Filter.ToLower() == "overdue")
                    {
                        var roadmapsOver = await query.ToListAsync(cancellationToken);
                        roadmapsOver = roadmapsOver.Where(r => IsOverdue(r)).ToList();
                        Console.WriteLine($"Filtered {roadmapsOver.Count} overdue roadmaps");
                        return CreatePaginatedResult(roadmapsOver, request);
                    }
                    else if (request.Filter.ToLower() == "neardue")
                    {
                        var roadmapsNear = await query.ToListAsync(cancellationToken);
                        roadmapsNear = roadmapsNear.Where(r => IsNearDue(r)).ToList();
                        Console.WriteLine($"Filtered {roadmapsNear.Count} neardue roadmaps");
                        return CreatePaginatedResult(roadmapsNear, request);
                    }
                    else
                    {
                        query = request.Filter.ToLower() switch
                        {
                            "draft" => query.Where(r => r.IsDraft),
                            "completed" => query.Where(r => r.IsCompleted),
                            _ => query
                        };
                    }
                }

                if (!string.IsNullOrEmpty(request.Search))
                {
                    query = query.Where(r => r.Title.ToLower().Contains(request.Search.ToLower()));
                }

                var totalCount = await query.CountAsync(cancellationToken);

                Console.WriteLine($"Total count after filters: {totalCount}");

                var roadmaps = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync(cancellationToken);

                var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

                return new PaginatedRoadmapResult<Roadmap>
                {
                    Items = roadmaps,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    CurrentPage = request.PageNumber,
                    PageSize = request.PageSize
                };
            }


            private bool IsOverdue(Roadmap roadmap)
            {
                var dueDate = GetLatestTaskDueDate(roadmap);
                Console.WriteLine($"Checking if roadmap is overdue, due date: {dueDate}");
                return dueDate.HasValue && dueDate.Value < DateTime.UtcNow;
            }

            private bool IsNearDue(Roadmap roadmap)
            {
                var dueDate = GetLatestTaskDueDate(roadmap);
                Console.WriteLine($"Checking if roadmap is near due, due date: {dueDate}");
                return dueDate.HasValue && dueDate.Value <= DateTime.UtcNow.AddDays(7) && dueDate.Value > DateTime.UtcNow;
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
                            if (task.IsDeleted)
                            {
                                Console.WriteLine($"Skipping task with ID {task.TaskId} because it is deleted.");
                                continue;
                            }

                            var taskEndDate = task.DateEnd;
                            Console.WriteLine($"Task due date: {taskEndDate}");

                            if ((!latestDate.HasValue || taskEndDate > latestDate))
                            {
                                latestDate = taskEndDate;
                            }

                        }
                    }
                }
                Console.WriteLine($"Latest task due date for roadmapxdfb: {latestDate}");
                return latestDate;
            }

            private PaginatedRoadmapResult<Roadmap> CreatePaginatedResult(List<Roadmap> roadmaps, Query request)
            {
                var totalCount = roadmaps.Count;
                var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

                return new PaginatedRoadmapResult<Roadmap>
                {
                    Items = roadmaps.Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize).ToList(),
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    CurrentPage = request.PageNumber,
                    PageSize = request.PageSize
                };
            }
        }

    }
}