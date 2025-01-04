using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System.Linq.Dynamic.Core;

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
            public string SortBy { get; set; } 
            public int Asc { get; set; } 
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

                var query = _context.Roadmaps
                    .Where(r => !r.IsDeleted)
                    .AsNoTracking()
                    .AsQueryable();

                if (request.CreatedAfter.HasValue)
                {
                    var startOfDay = request.CreatedAfter.Value.Date;
                    var endOfDay = startOfDay.AddDays(1).AddTicks(-1);
                    query = query.Where(r => r.CreatedAt >= startOfDay);
                    Console.WriteLine($"Filtering created after: {startOfDay} to {endOfDay}");
                }

                if (!string.IsNullOrEmpty(request.Filter))
                {
                    Console.WriteLine($"Applying filter: {request.Filter.ToLower()}");

                    query = request.Filter.ToLower() switch
                    {
                        "draft" => query.Where(r => r.IsDraft),
                        "completed" => query.Where(r => r.IsCompleted),
                        "neardue" => query.Where(r =>
                            r.Milestones.SelectMany(m => m.Sections)
                                        .SelectMany(s => s.ToDoTasks)
                                        .Max(t => (DateTime?)t.DateEnd) <= DateTime.UtcNow.AddDays(7) &&
                            r.Milestones.SelectMany(m => m.Sections)
                                        .SelectMany(s => s.ToDoTasks)
                                        .Max(t => (DateTime?)t.DateEnd) > DateTime.UtcNow),
                        "overdue" => query.Where(r =>
                            r.Milestones.SelectMany(m => m.Sections)
                                        .SelectMany(s => s.ToDoTasks)
                                        .Max(t => (DateTime?)t.DateEnd) < DateTime.UtcNow),
                        _ => query
                    };
                }

                if (!string.IsNullOrEmpty(request.Search))
                {
                    query = query.Where(r => r.Title.ToLower().Contains(request.Search.ToLower()));
                }


                if (!string.IsNullOrEmpty(request.SortBy))
                {
                    if (request.Asc != 1 && request.Asc != 0)
                    {
                        throw new Exception("Order Type must be 1 (asc) or 0 (desc)");
                    }

                    string sortOrder = request.Asc == 1 ? "ascending" : "descending";
                    string sortExpression = $"{request.SortBy} {sortOrder}";

                    Console.WriteLine("Sort By: " + request.SortBy);
                    Console.WriteLine("Direction: " + sortOrder);

                    try
                    {
                        query = query.OrderBy(sortExpression);
                        Console.WriteLine(query);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Invalid sort field '{request.SortBy}' or order '{sortOrder}'.", ex);
                    }
                }



                var totalCount = await query.CountAsync(cancellationToken);


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
                if (!dueDate.HasValue)
                {
                    Console.WriteLine($"No due date found for roadmap {roadmap.RoadmapId}");
                    return false;
                }
                return dueDate.Value < DateTime.UtcNow;
            }

            private bool IsNearDue(Roadmap roadmap)
            {
                var dueDate = GetLatestTaskDueDate(roadmap);
                if (!dueDate.HasValue)
                {
                    return false;
                }
                return dueDate.Value <= DateTime.UtcNow.AddDays(7) && dueDate.Value > DateTime.UtcNow;
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
                            var taskEndDate = task.DateEnd;

                            if ((!latestDate.HasValue || taskEndDate > latestDate))
                            {
                                latestDate = taskEndDate;
                            }

                        }
                    }
                }
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