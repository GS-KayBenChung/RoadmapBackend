using Application.DTOs;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

public class Edit
{
    public class Command : IRequest
    {
        public Guid RoadmapId { get; set; }
        public RoadmapDto RoadmapDto { get; set; }
    }

    public class Handler : IRequestHandler<Edit.Command>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task Handle(Edit.Command request, CancellationToken cancellationToken)
        {
            var roadmap = await _context.Roadmaps
                .Include(r => r.Milestones)
                    .ThenInclude(m => m.Sections)
                        .ThenInclude(s => s.ToDoTasks)
                .FirstOrDefaultAsync(r => r.RoadmapId == request.RoadmapId, cancellationToken);

            if (roadmap == null)
            {
                throw new Exception("Roadmap not found.");
            }

            // Update the roadmap fields
            roadmap.Title = request.RoadmapDto.Title;
            roadmap.Description = request.RoadmapDto.Description;
            roadmap.UpdatedAt = DateTime.UtcNow;

            // Update milestones, sections, and tasks
            foreach (var milestoneDto in request.RoadmapDto.Milestones)
            {
                var milestone = roadmap.Milestones.FirstOrDefault(m => m.Name == milestoneDto.Name);
                if (milestone != null)
                {
                    milestone.Name = milestoneDto.Name;
                    milestone.Description = milestoneDto.Description;
                    milestone.UpdatedAt = DateTime.UtcNow;

                    foreach (var sectionDto in milestoneDto.Sections)
                    {
                        var section = milestone.Sections.FirstOrDefault(s => s.Name == sectionDto.Name);
                        if (section != null)
                        {
                            section.Name = sectionDto.Name;
                            section.Description = sectionDto.Description;
                            section.UpdatedAt = DateTime.UtcNow;

                            foreach (var taskDto in sectionDto.Tasks)
                            {
                                var task = section.ToDoTasks.FirstOrDefault(t => t.Name == taskDto.Name);
                                if (task != null)
                                {
                                    task.Name = taskDto.Name;
                                    task.DateStart = taskDto.DateStart;
                                    task.DateEnd = taskDto.DateEnd;
                                    task.UpdatedAt = DateTime.UtcNow;
                                }
                            }
                        }
                    }
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
