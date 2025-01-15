using Domain.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.RoadmapActivities
{
    public class PatchRoadmap
    {
        public class Command : IRequest
        {
            public Guid RoadmapId { get; set; }
            public RoadmapUpdateDto UpdateDto { get; set; }
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
                Console.WriteLine($"HANDLE: Updating Roadmap {request.RoadmapId}");

                var roadmap = await _context.Roadmaps.FirstOrDefaultAsync(r => r.RoadmapId == request.RoadmapId, cancellationToken);
                if (roadmap == null)
                {
                    Console.WriteLine("Roadmap not found.");
                    throw new InvalidOperationException($"No Roadmap with ID '{request.RoadmapId}' found.");
                }
                Console.WriteLine($"Found Roadmap: {roadmap.Title}");

                if (request.UpdateDto.Roadmap != null)
                {
                    Console.WriteLine($"Updating Title: {request.UpdateDto.Roadmap.Title}, Description: {request.UpdateDto.Roadmap.Description}");
                    roadmap.Title = request.UpdateDto.Roadmap.Title ?? roadmap.Title;
                    roadmap.Description = request.UpdateDto.Roadmap.Description ?? roadmap.Description;
                    roadmap.UpdatedAt = DateTime.UtcNow;
                }

                if (request.UpdateDto.Milestones?.Any() == true)
                {
                    Console.WriteLine($"Updating {request.UpdateDto.Milestones.Count} milestones");
                    var milestoneIds = request.UpdateDto.Milestones.Select(m => m.MilestoneId).ToList();
                    var milestones = await _context.Milestones.Where(m => milestoneIds.Contains(m.MilestoneId)).ToListAsync(cancellationToken);

                    foreach (var milestone in milestones)
                    {
                        var update = request.UpdateDto.Milestones.FirstOrDefault(m => m.MilestoneId == milestone.MilestoneId);
                        Console.WriteLine($"Updating Milestone: {milestone.MilestoneId} -> Name: {update.Name}, Description: {update.Description}");
                        milestone.Name = update.Name ?? milestone.Name;
                        milestone.Description = update.Description ?? milestone.Description;
                        milestone.UpdatedAt = DateTime.UtcNow;
                    }
                }

                if (request.UpdateDto.Sections?.Any() == true)
                {
                    Console.WriteLine($"Updating {request.UpdateDto.Sections.Count} sections");
                    var sectionIds = request.UpdateDto.Sections.Select(s => s.SectionId).ToList();
                    var sections = await _context.Sections.Where(s => sectionIds.Contains(s.SectionId)).ToListAsync(cancellationToken);

                    foreach (var section in sections)
                    {
                        var update = request.UpdateDto.Sections.FirstOrDefault(s => s.SectionId == section.SectionId);
                        Console.WriteLine($"Updating Section: {section.SectionId} -> Name: {update.Name}, Description: {update.Description}");
                        section.Name = update.Name ?? section.Name;
                        section.Description = update.Description ?? section.Description;
                        section.UpdatedAt = DateTime.UtcNow;
                    }
                }

                if (request.UpdateDto.Tasks?.Any() == true)
                {
                    Console.WriteLine($"Updating {request.UpdateDto.Tasks.Count} tasks");
                    var taskIds = request.UpdateDto.Tasks.Select(t => t.TaskId).ToList();
                    var tasks = await _context.ToDoTasks.Where(t => taskIds.Contains(t.TaskId)).ToListAsync(cancellationToken);

                    foreach (var task in tasks)
                    {
                        var update = request.UpdateDto.Tasks.FirstOrDefault(t => t.TaskId == task.TaskId);
                        Console.WriteLine($"Updating Task: {task.TaskId} -> Name: {update.Name}, Start: {update.DateStart}, End: {update.DateEnd}");
                        task.Name = update.Name ?? task.Name;
                        task.DateStart = update.DateStart ?? task.DateStart;
                        task.DateEnd = update.DateEnd ?? task.DateEnd;
                        task.UpdatedAt = DateTime.UtcNow;
                    }
                }

                try
                {
                    var result = await _context.SaveChangesAsync(cancellationToken);
                    Console.WriteLine($"SaveChangesAsync result: {result}");
                }
                catch (DbUpdateException ex)
                {
                    Console.WriteLine($"Database update failed: {ex.Message}");
                    throw new Exception("Database update failed.", ex);
                }
            }
        }
    }
}
