using Application.Validator;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Serilog;

namespace Application.RoadmapActivities
{
    public class Delete
    {
        public class Command : IRequest
        {
            public Guid Id { get; set; }
        }

        public class Handler : IRequestHandler<Command>
        {
            private readonly DataContext _context;
            private readonly IValidationService _validationService;

            public Handler(DataContext context, IValidationService validationService)
            {
                _context = context;
                _validationService = validationService;
            }

            public async Task Handle(Command request, CancellationToken cancellationToken)
            {
                await _validationService.ValidateAsync(request, cancellationToken);

                var traceId = Guid.NewGuid().ToString();
                Log.Information("[{TraceId}] Started deleting roadmap with ID: {RoadmapId}", traceId, request.Id);

                var roadmap = await _context.Roadmaps
                    .Include(r => r.Milestones)
                        .ThenInclude(m => m.Sections)
                            .ThenInclude(s => s.ToDoTasks)
                    .FirstOrDefaultAsync(r => r.RoadmapId == request.Id, cancellationToken);

                if (roadmap == null)
                {
                    Log.Warning("[{TraceId}] Roadmap with ID {RoadmapId} not found.", traceId, request.Id);
                    throw new ValidationException(new List<FluentValidation.Results.ValidationFailure>
                    {
                        new("RoadmapId", "Roadmap not found.")
                    });
                }

                if (roadmap.IsDeleted)
                {
                    Log.Warning("[{TraceId}] Roadmap with ID {RoadmapId} is already deleted.", traceId, request.Id);
                    throw new ValidationException(new List<FluentValidation.Results.ValidationFailure>
                    {
                        new("RoadmapId", "Roadmap is already deleted.")
                    });
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

                Log.Information("[{TraceId}] Successfully deleted roadmap with ID {RoadmapId}", traceId, request.Id);
            }
        }
    }
}
