using Application.DTOs;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System.Linq.Dynamic.Core;

namespace Application.AuditActivities
{
    public class GetLogs
    {
        public class Query : IRequest<PaginatedLogResult<RoadmapLogsDto>>
        {
            public string Filter { get; set; }
            public string Search { get; set; }
            public DateTime? CreatedOn { get; set; }
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
            public string SortBy { get; set; }
            public int Asc { get; set; }
        }
        public class Handler : IRequestHandler<Query, PaginatedLogResult<RoadmapLogsDto>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<PaginatedLogResult<RoadmapLogsDto>> Handle(Query request, CancellationToken cancellationToken)
            {

                var query = _context.AuditLogs
                    .Include(log => log.User)
                    .AsQueryable();

                if (request.CreatedOn.HasValue)
                {
                    var targetDate = request.CreatedOn.Value.Date;
                    query = query.Where(r => r.CreatedAt.Date == targetDate);
                }

                if (!string.IsNullOrEmpty(request.Filter))
                {
                    
                    if (!string.IsNullOrEmpty(request.Filter))
                    {
                        var filter = request.Filter.ToLower();

                        query = filter switch
                        {
                            "created" => query.Where(log => log.ActivityAction.ToLower().Contains("create")),
                            "updated" => query.Where(log => log.ActivityAction.ToLower().Contains("update")),
                            "deleted" => query.Where(log => log.ActivityAction.ToLower().Contains("delete")),
                            _ => query
                        };
                    }
                }

                if (!string.IsNullOrEmpty(request.Search))
                {
                    var searchLower = request.Search.ToLower();
                    query = query.Where(log =>
                        log.ActivityAction.ToLower().Contains(searchLower) ||
                        log.User.Name.ToLower().Contains(searchLower));
                }


                if (!string.IsNullOrEmpty(request.SortBy))
                {

                    var allowedSortFields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "activityaction", "ActivityAction" },
                        { "createdat", "CreatedAt" },
                        { "name", "User.Name" }
                    };

                    if (!allowedSortFields.TryGetValue(request.SortBy, out var sortField))
                    {
                        throw new Exception($"Invalid SortBy field: {request.SortBy}");
                    }

                    if (request.Asc != 1 && request.Asc != 0)
                    {
                        throw new Exception("Order Type must be 1 (asc) or 0 (desc)");
                    }

                    var sortOrder = request.Asc == 1 ? "ascending" : "descending";
                    var sortExpression = $"{sortField} {sortOrder}";

                    try
                    {
                        query = query.OrderBy(sortExpression);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Error applying sort expression '{sortExpression}': {ex.Message}", ex);
                    }
                }

                var totalCount = await query.CountAsync(cancellationToken);
                
                var logs = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(log => new RoadmapLogsDto
                    {
                        LogId = log.LogId,
                        UserId = log.UserId,
                        UserName = log.User.Name,
                        ActivityAction = log.ActivityAction,
                        CreatedAt = log.CreatedAt,
                    })
                    .ToListAsync(cancellationToken);

                var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

                return new PaginatedLogResult<RoadmapLogsDto>
                {
                    Items = logs,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    CurrentPage = request.PageNumber,
                    PageSize = request.PageSize
                };
            }
        }
    }
}