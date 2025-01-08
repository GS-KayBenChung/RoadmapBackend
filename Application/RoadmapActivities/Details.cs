
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Serilog;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

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
                var traceId = Guid.NewGuid().ToString();

                var roadmap = await _context.Roadmaps.FindAsync(new object[] { request.Id }, cancellationToken);

                var roadmapJson = JsonSerializer.Serialize(roadmap, new JsonSerializerOptions
                {
                    ReferenceHandler = ReferenceHandler.Preserve, 
                });
                Log.Information("[{Timestamp:yyyy-MM-dd HH:mm:ss}] [INFO] [TraceId: {TraceId}] Get Roadmap: {Roadmap}",
                DateTime.UtcNow,
                traceId,
                roadmapJson);

                return roadmap;
            }
        }

    }
}
