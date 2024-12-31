using Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

public class GetLogs
{
    public class Query : IRequest<List<RoadmapLogsDto>>
    {
        public string Filter { get; set; } 
        public string Search { get; set; }  
    }

    public class Handler : IRequestHandler<Query, List<RoadmapLogsDto>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<List<RoadmapLogsDto>> Handle(Query request, CancellationToken cancellationToken)
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

            var logs = await query
                .OrderByDescending(log => log.CreatedAt)
                .Select(log => new RoadmapLogsDto
                {
                    LogId = log.LogId,
                    UserId = log.UserId,
                    ActivityAction = log.ActivityAction,
                    CreatedAt = log.CreatedAt
                })
                .ToListAsync(cancellationToken);

            return logs;
        }

    }

}