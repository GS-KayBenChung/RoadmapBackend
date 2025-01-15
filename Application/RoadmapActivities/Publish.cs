using Domain.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Serilog;

namespace Application.RoadmapActivities
{
    public class Publish
    {
        public class Command : IRequest<StatusDto>
        {
            public Guid Id { get; set; }
        }

        public class Handler : IRequestHandler<Command, StatusDto>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<StatusDto> Handle(Command request, CancellationToken cancellationToken)
            {
                var traceId = Guid.NewGuid().ToString();

                Log.Information("[{Timestamp:yyyy-MM-dd HH:mm:ss}] [INFO] [TraceId: {TraceId}] Started publishing roadmap with ID: {RoadmapId}",
                    DateTime.UtcNow, traceId, request.Id);

                var roadmap = await _context.Roadmaps
                    .FirstOrDefaultAsync(r => r.RoadmapId == request.Id, cancellationToken);

                if (roadmap == null || roadmap.IsDeleted)
                {
                    Log.Warning("[{Timestamp:yyyy-MM-dd HH:mm:ss}] [WARNING] [TraceId: {TraceId}] Roadmap with ID: {RoadmapId} not found or deleted.",
                        DateTime.UtcNow, traceId, request.Id);

                    return new StatusDto
                    {
                        err = $"Cannot publish roadmap because it does not exist or is deleted.",
                        status = "401"
                    };
                }

                roadmap.IsDraft = false;
                roadmap.UpdatedAt = DateTime.UtcNow;

                _context.Roadmaps.Update(roadmap);

                await _context.SaveChangesAsync(cancellationToken);

                Log.Information("[{Timestamp:yyyy-MM-dd HH:mm:ss}] [INFO] [TraceId: {TraceId}] Successfully published roadmap with ID: {RoadmapId}",
                    DateTime.UtcNow, traceId, request.Id);

                return new StatusDto { err = "None", status = "Success" };
            }
        }
    }
}
