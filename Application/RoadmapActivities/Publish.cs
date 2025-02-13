using Application.Validator;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Serilog;

namespace Application.RoadmapActivities
{
    public class Publish
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

                Log.Information("[{Timestamp:yyyy-MM-dd HH:mm:ss}] [INFO] [TraceId: {TraceId}] Started publishing roadmap with ID: {RoadmapId}",
                    DateTime.UtcNow, traceId, request.Id);

                var roadmap = await _context.Roadmaps
                    .Include(r => r.Milestones)
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
                    Log.Warning("[{TraceId}] Attempted to publish deleted roadmap {RoadmapId}.", traceId, request.Id);
                    throw new ValidationException(new List<FluentValidation.Results.ValidationFailure>
                    {
                        new("RoadmapId", "This roadmap has been deleted and cannot be published.")
                    });
                }

                if (!roadmap.IsDraft)
                {
                    Log.Warning("[{TraceId}] Roadmap {RoadmapId} is already published.", traceId, request.Id);
                    throw new ValidationException(new List<FluentValidation.Results.ValidationFailure>
                    {
                        new("RoadmapId", "Roadmap is already published.")
                    });
                }


                if (!roadmap.Milestones.Any())
                {
                    Log.Warning("[{TraceId}] Roadmap {RoadmapId} cannot be published without milestones.", traceId, request.Id);
                    throw new ValidationException(new List<FluentValidation.Results.ValidationFailure>
                    {
                        new("Milestones", "A roadmap must have at least one milestone before publishing.")
                    });
                }

                roadmap.IsDraft = false;
                roadmap.UpdatedAt = DateTime.UtcNow;
                _context.Roadmaps.Update(roadmap);

                await _context.SaveChangesAsync(cancellationToken);

                Log.Information("[{TraceId}] Successfully published roadmap with ID {RoadmapId}", traceId, request.Id);
            }
        }
    }
}
