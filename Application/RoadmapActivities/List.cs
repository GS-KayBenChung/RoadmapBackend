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
                    .AsNoTracking()
                    .AsQueryable();

                if (!string.IsNullOrEmpty(request.Filter))
                {
                    query = request.Filter.ToLower() switch
                    {
                        "draft" => query.Where(r => r.IsDraft),
                        "completed" => query.Where(r => r.IsCompleted),
                        //"inprogress" => query.Where(r => !r.IsCompleted && !r.IsDraft),
                        //"overdue" => query.Where(r => r.DueDate < DateTime.UtcNow && !r.IsCompleted),
                        _ => query 
                    };
                }

                if (!string.IsNullOrEmpty(request.Search))
                {
                    query = query.Where(r =>
                        r.Title.ToLower().Contains(request.Search.ToLower()));
                }
                //Console.WriteLine($"Result count: {query.Count()}");

                return await query.ToListAsync(cancellationToken);
            }
        }
    }
}
