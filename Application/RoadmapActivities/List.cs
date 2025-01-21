using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Serilog;
using System.Diagnostics;
using System.Linq.Dynamic.Core;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Application.RoadmapActivities
{
    public class List
    {
        public class Query : IRequest<PaginatedRoadmapResult<Roadmap>>
        {
            public string Filter { get; set; }
            public string Search { get; set; }
            public DateTime? CreatedAfter { get; set; }
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
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
                var traceId = Guid.NewGuid().ToString();

                var query = _context.Roadmaps
                    .Where(r => !r.IsDeleted)
                    .AsNoTracking()
                    .AsQueryable();

                if (request.CreatedAfter.HasValue)
                {
                    var startOfDay = request.CreatedAfter.Value.Date;
                    var endOfDay = startOfDay.AddDays(1).AddTicks(-1);
                    query = query.Where(r => r.CreatedAt >= startOfDay);
                    
                }

                if (!string.IsNullOrEmpty(request.Filter))
                {

                    query = request.Filter.ToLower() switch
                    {
                        "draft" => query.Where(r => r.IsDraft),
                        "published" => query.Where(r => !r.IsDraft),
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

                    try
                    {
                        query = query.OrderBy(sortExpression);
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

                //Log.Information("[{Timestamp:yyyy-MM-dd HH:mm:ss}] [INFO] [TraceId: {TraceId}] Get Roadmap: {Roadmap}",
                Log.Information("Get Roadmap: {Roadmap}",

                //DateTime.UtcNow,
                //traceId,
                JsonSerializer.Serialize(roadmaps, new JsonSerializerOptions
                {
                    ReferenceHandler = ReferenceHandler.Preserve
                }));

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