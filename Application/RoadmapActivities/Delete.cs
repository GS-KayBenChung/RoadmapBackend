using Application.Dto;
using Domain.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Serilog;

namespace Application.RoadmapActivities
{
    public class Delete
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

                Log.Information("[{Timestamp:yyyy-MM-dd HH:mm:ss}] [INFO] [TraceId: {TraceId}] Started deleting roadmap with ID: {RoadmapId}",
                    DateTime.UtcNow, traceId, request.Id);

                var roadmap = await _context.Roadmaps
                    .Include(r => r.Milestones)
                        .ThenInclude(m => m.Sections)
                            .ThenInclude(s => s.ToDoTasks)
                    .FirstOrDefaultAsync(r => r.RoadmapId == request.Id, cancellationToken);

                if (roadmap == null || roadmap.IsDeleted == true)
                {
                    Log.Warning("[{Timestamp:yyyy-MM-dd HH:mm:ss}] [WARNING] [TraceId: {TraceId}] Roadmap with ID: {RoadmapId} not found or already deleted.",
                        DateTime.UtcNow, traceId, request.Id);

                    return new StatusDto
                    {
                        err = $"Cannot delete roadmap because roadmap with this ID does not exist in the database.",
                        status = "401"
                    };
                }

                roadmap.IsDeleted = true;

                foreach (var milestone in roadmap.Milestones)
                {
                    milestone.IsDeleted = true;

                    foreach (var section in milestone.Sections)
                    {
                        section.IsDeleted = true;

                        foreach (var task in section.ToDoTasks)
                        {
                            task.IsDeleted = true;
                        }
                    }
                }

                _context.Roadmaps.Update(roadmap);

                await _context.SaveChangesAsync(cancellationToken);

                Log.Information("[{Timestamp:yyyy-MM-dd HH:mm:ss}] [INFO] [TraceId: {TraceId}] Successfully deleted roadmap with ID: {RoadmapId}",
                    DateTime.UtcNow, traceId, request.Id);

                return new StatusDto { err = "None", status = "Success" };
            }
        }
    }
}
