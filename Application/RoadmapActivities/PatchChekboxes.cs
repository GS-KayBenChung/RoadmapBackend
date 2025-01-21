using Domain;
using Domain.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.RoadmapActivities
{
    public class PatchCompletionStatus
    {
        public class Command : IRequest
        {
            public string EntityType { get; set; } = string.Empty;
            public CompletionStatusDto UpdateDto { get; set; }
            public Guid Id { get; set; } 
            public bool? IsCompleted { get; set; }  
            public int? Progression { get; set; }   
        }

        public class Handler : IRequestHandler<Command>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task Handle(Command request, CancellationToken cancellationToken)
            {



                switch (request.EntityType.ToLower())
                {
                    case "roadmap":
                        var roadmap = await _context.Roadmaps.FirstOrDefaultAsync(r => r.RoadmapId == request.UpdateDto.Id, cancellationToken);
                        if (roadmap == null) throw new InvalidOperationException("Roadmap not found.");

                        if (request.UpdateDto.Progress.HasValue)
                        {
                            roadmap.OverallProgress = request.UpdateDto.Progress.Value;
                            roadmap.UpdatedAt = DateTime.UtcNow;
                        }
                        if (request.UpdateDto.IsCompleted.HasValue)
                        {
                            roadmap.IsCompleted = request.UpdateDto.IsCompleted.Value;
                            roadmap.UpdatedAt = DateTime.UtcNow;
                        }
                        break;

                    case "milestone":
                        var milestone = await _context.Milestones.FirstOrDefaultAsync(m => m.MilestoneId == request.UpdateDto.Id, cancellationToken);
                        if (milestone == null) throw new InvalidOperationException("Milestone not found.");

                        if (request.UpdateDto.Progress.HasValue)
                        {
                            milestone.MilestoneProgress = request.UpdateDto.Progress.Value;
                            milestone.UpdatedAt = DateTime.UtcNow;
                        }
                        if (request.UpdateDto.IsCompleted.HasValue)
                        {
                            milestone.IsCompleted = request.UpdateDto.IsCompleted.Value;
                            milestone.UpdatedAt = DateTime.UtcNow;
                        }
                        break;

                    case "section":
                        var section = await _context.Sections.FirstOrDefaultAsync(s => s.SectionId == request.UpdateDto.Id, cancellationToken);
                        if (section == null) throw new InvalidOperationException("Section not found.");

                        if (request.UpdateDto.IsCompleted.HasValue)
                        {
                            section.IsCompleted = request.UpdateDto.IsCompleted.Value;
                            section.UpdatedAt = DateTime.UtcNow;
                        }
                        break;

                    case "task":
                        var task = await _context.ToDoTasks.FirstOrDefaultAsync(t => t.TaskId == request.UpdateDto.Id, cancellationToken);
                        if (task == null) throw new InvalidOperationException("Task not found.");

                        if (request.UpdateDto.IsCompleted.HasValue)
                        {
                            task.IsCompleted = request.UpdateDto.IsCompleted.Value;
                            task.UpdatedAt = DateTime.UtcNow;
                        }

                        break;

                    default:
                        throw new InvalidOperationException("Invalid entity type.");
                }

                await _context.SaveChangesAsync(cancellationToken);
            }

        }
    }
}