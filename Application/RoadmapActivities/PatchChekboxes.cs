using Application.Validator;
using Domain.Dtos;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Serilog;

namespace Application.RoadmapActivities
{
    public class PatchCompletionStatus
    {
        public class Command : IRequest
        {
            public string EntityType { get; set; } = string.Empty;
            public CompletionStatusDto UpdateDto { get; set; }
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
                Log.Information("Processing completion update for {EntityType} ID: {Id}", request.EntityType, request.UpdateDto.Id);

                switch (request.EntityType.ToLower())
                {
                    case "roadmap":
                        var roadmap = await _context.Roadmaps.FirstOrDefaultAsync(r => r.RoadmapId == request.UpdateDto.Id, cancellationToken);
                        if (roadmap == null) throw new ValidationException("Roadmap not found.");

                        roadmap.OverallProgress = request.UpdateDto.Progress ?? roadmap.OverallProgress;
                        roadmap.IsCompleted = request.UpdateDto.IsCompleted ?? roadmap.IsCompleted;
                        roadmap.UpdatedAt = DateTime.UtcNow;
                        break;

                    case "milestone":
                        var milestone = await _context.Milestones.FirstOrDefaultAsync(m => m.MilestoneId == request.UpdateDto.Id, cancellationToken);
                        if (milestone == null) throw new ValidationException("Milestone not found.");

                        milestone.MilestoneProgress = request.UpdateDto.Progress ?? milestone.MilestoneProgress;
                        milestone.IsCompleted = request.UpdateDto.IsCompleted ?? milestone.IsCompleted;
                        milestone.UpdatedAt = DateTime.UtcNow;
                        break;

                    case "section":
                        var section = await _context.Sections.FirstOrDefaultAsync(s => s.SectionId == request.UpdateDto.Id, cancellationToken);
                        if (section == null) throw new ValidationException("Section not found.");

                        section.IsCompleted = request.UpdateDto.IsCompleted ?? section.IsCompleted;
                        section.UpdatedAt = DateTime.UtcNow;
                        break;

                    case "task":
                        var task = await _context.ToDoTasks.FirstOrDefaultAsync(t => t.TaskId == request.UpdateDto.Id, cancellationToken);
                        if (task == null) throw new ValidationException("Task not found.");

                        task.IsCompleted = request.UpdateDto.IsCompleted ?? task.IsCompleted;
                        task.UpdatedAt = DateTime.UtcNow;
                        break;
                }

                await _context.SaveChangesAsync(cancellationToken);
                Log.Information("Successfully updated {EntityType} ID: {Id}", request.EntityType, request.UpdateDto.Id);
            }
        }
    }
}
