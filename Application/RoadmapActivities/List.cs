using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.RoadmapActivities
{
    public class List
    {
        public class Query : IRequest<List<Roadmap>>
        {
            public string Filter { get; set; }
            public string Search { get; set; }
            public DateTime? CreatedAfter { get; set; }
        }

        public class Handler : IRequestHandler<Query, List<Roadmap>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<List<Roadmap>> Handle(Query request, CancellationToken cancellationToken)
            {
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
                }

                if (!string.IsNullOrEmpty(request.Filter))
                {
                    query = request.Filter.ToLower() switch
                    {
                        "draft" => query.Where(r => r.IsDraft),
                        "completed" => query.Where(r => r.IsCompleted),
                        _ => query
                    };
                }

                if (!string.IsNullOrEmpty(request.Search))
                {
                    query = query.Where(r =>
                        r.Title.ToLower().Contains(request.Search.ToLower()));
                }

                return await query.ToListAsync(cancellationToken);
            }
        }
    }
}