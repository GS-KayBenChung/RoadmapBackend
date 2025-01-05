
using Domain;
using MediatR;
using Persistence;
using Serilog;
using System.Diagnostics;
using System.Text.Json;

namespace Application.RoadmapActivities
{
    public class Details
    {
        public class Query : IRequest<Roadmap>
        {
            public Guid Id { get; set; }
        }

        public class Handler : IRequestHandler<Details.Query, Roadmap>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Roadmap> Handle(Details.Query request, CancellationToken cancellationToken)
            {
               
                var roadmap = await _context.Roadmaps.FindAsync(new object[] { request.Id }, cancellationToken);
                
                return roadmap;
            }
        }

    }
}
