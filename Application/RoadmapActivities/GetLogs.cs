using Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

public class GetLogs
{
    public class Query : IRequest<PaginatedLogResult<RoadmapLogsDto>>
    {
        public string Filter { get; set; } 
        public string Search { get; set; }
        public int PageNumber { get; set; } = 1; 
        public int PageSize { get; set; } = 10; 
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
            var query = _context.AuditLogs.AsQueryable();

        
            if (!string.IsNullOrEmpty(request.Search))
            {
                query = query.Where(log =>
                    log.ActivityAction.Contains(request.Search) || log.LogId.ToString().Contains(request.Search));
            }

         
            if (!string.IsNullOrEmpty(request.Filter))
            {
                var filter = request.Filter.ToLower();

                if (filter == "created")
                {
                    query = query.Where(log => log.ActivityAction.ToLower().Contains("create"));
                }
                else if (filter == "updated")
                {
                    query = query.Where(log => log.ActivityAction.ToLower().Contains("update"));
                }
                else if (filter == "deleted")
                {
                    query = query.Where(log => log.ActivityAction.ToLower().Contains("delete"));
                }
            }

          
            var totalCount = await query.CountAsync(cancellationToken);

           
            var logs = await query
                .OrderByDescending(log => log.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(log => new RoadmapLogsDto
                {
                    LogId = log.LogId,
                    UserId = log.UserId,
                    ActivityAction = log.ActivityAction,
                    CreatedAt = log.CreatedAt
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