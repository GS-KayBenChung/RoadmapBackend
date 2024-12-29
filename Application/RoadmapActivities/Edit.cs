using Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.RoadmapActivities
{
    public class UpdateRoadmap
    {
        public class Command : IRequest
        {
            public Guid Id { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public bool IsDraft { get; set; }
            public List<MilestoneResponseDto> Milestones { get; set; }
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
                var roadmap = await _context.Roadmaps
                    .Include(r => r.Milestones)
                    .ThenInclude(m => m.Sections)
                    .ThenInclude(s => s.ToDoTasks)
                    .FirstOrDefaultAsync(r => r.RoadmapId == request.Id, cancellationToken);

                if (roadmap == null)
                {
                    throw new Exception("Roadmap not found");
                }

                roadmap.Title = request.Title;
                roadmap.Description = request.Description;
                roadmap.IsDraft = request.IsDraft;
                roadmap.UpdatedAt = DateTime.UtcNow;

                foreach (var milestoneDto in request.Milestones)
                {

                    var milestone = roadmap.Milestones.FirstOrDefault(m => m.MilestoneId == milestoneDto.MilestoneId);
                    if (milestone != null)
                    {
                        milestone.Name = milestoneDto.Name;
                        milestone.Description = milestoneDto.Description;
                        milestone.UpdatedAt = DateTime.UtcNow;

                        foreach (var sectionDto in milestoneDto.Sections)
                        {
                            var section = milestone.Sections.FirstOrDefault(s => s.SectionId == sectionDto.SectionId);
                            if (section != null)
                            {
                                section.Name = sectionDto.Name;
                                section.Description = sectionDto.Description;
                                section.UpdatedAt = DateTime.UtcNow;

                                foreach (var taskDto in sectionDto.Tasks)
                                {
                                    var task = section.ToDoTasks.FirstOrDefault(t => t.TaskId == taskDto.TaskId);
                                    if (task != null)
                                    {
                                        task.Name = taskDto.Name;
                                        task.DateStart = taskDto.DateStart;
                                        task.DateEnd = taskDto.DateEnd;
                                        task.IsCompleted = taskDto.IsCompleted;
                                        task.UpdatedAt = DateTime.UtcNow;
                                    }
                                }
                            }
                        }
                    }
                }

                try
                {
                    var success = await _context.SaveChangesAsync(cancellationToken);
                    Console.WriteLine($"Number of rows updated: {success}");
                    if (success <= 0) throw new Exception("Failed to update roadmap");
                }
                catch (DbUpdateException ex)
                {
                    Console.WriteLine($"Database update error: {ex.InnerException?.Message}");
                    throw;
                }
            }

        }
    }
}
