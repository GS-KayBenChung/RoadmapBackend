using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.RoadmapActivities
{
    public class UpdateCheckedBoxes
    {
        public class Command : IRequest
        {
            public Guid Id { get; set; }
            public string Type { get; set; }
            public bool IsChecked { get; set; }
            public int? Index { get; set; }
            public int? ParentIndex { get; set; }
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
                    throw new Exception("Roadmap not found");

                if (request.Type == "roadmap")
                {
                    roadmap.IsCompleted = request.IsChecked;
                    foreach (var milestone in roadmap.Milestones)
                    {
                        milestone.IsCompleted = request.IsChecked;
                        foreach (var section in milestone.Sections)
                        {
                            section.IsCompleted = request.IsChecked;
                            foreach (var task in section.ToDoTasks)
                            {
                                task.IsCompleted = request.IsChecked;
                            }
                        }
                    }
                }
                else if (request.Type == "milestone" && request.Index.HasValue)
                {
                    var milestone = roadmap.Milestones.ElementAt(request.Index.Value);
                    milestone.IsCompleted = request.IsChecked;
                    foreach (var section in milestone.Sections)
                    {
                        section.IsCompleted = request.IsChecked;
                        foreach (var task in section.ToDoTasks)
                        {
                            task.IsCompleted = request.IsChecked;
                        }
                    }
                }
                else if (request.Type == "section" && request.Index.HasValue && request.ParentIndex.HasValue)
                {
                    
                    var milestone = roadmap.Milestones.ElementAt(request.ParentIndex.Value);
                    milestone.IsCompleted = milestone.Sections.All(s => s.IsCompleted);
                    var section = milestone.Sections.ElementAt(request.Index.Value);
                    section.IsCompleted = request.IsChecked;
                    foreach (var task in section.ToDoTasks)
                    {
                        task.IsCompleted = request.IsChecked;
                    }
                }
                else if (request.Type == "task" && request.Index.HasValue && request.ParentIndex.HasValue)
                {
                    var milestone = roadmap.Milestones.ElementAt(request.ParentIndex.Value);
                    milestone.IsCompleted = milestone.Sections.All(s => s.IsCompleted);
                    var section = milestone.Sections.ElementAt(request.ParentIndex.Value);
                    section.IsCompleted = section.ToDoTasks.All(t => t.IsCompleted);
                    var task = section.ToDoTasks.ElementAt(request.Index.Value);
                    task.IsCompleted = request.IsChecked;
                }

                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
