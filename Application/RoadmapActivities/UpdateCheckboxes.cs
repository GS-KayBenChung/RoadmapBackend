using Domain;
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
            public int? GrandParentIndex { get; set; }
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

                DateTime now = DateTime.UtcNow;

                if (request.Type == "roadmap")
                {
                    roadmap.IsCompleted = request.IsChecked;
                    roadmap.UpdatedAt = now;

                    foreach (var milestone in roadmap.Milestones)
                    {
                        milestone.IsCompleted = request.IsChecked;
                        milestone.UpdatedAt = now;

                        foreach (var section in milestone.Sections)
                        {
                            section.IsCompleted = request.IsChecked;
                            section.UpdatedAt = now;

                            foreach (var task in section.ToDoTasks)
                            {
                                task.IsCompleted = request.IsChecked;
                                task.UpdatedAt = now;
                            }
                        }

                        milestone.MilestoneProgress = CalculateMilestoneProgress(milestone); 
                    }

                    roadmap.OverallProgress = CalculateRoadmapProgress(roadmap);
                    if (roadmap.OverallProgress == 100) roadmap.IsCompleted = true;
                }
                else if (request.Type == "milestone" && request.Index.HasValue)
                {


                    var milestone = roadmap.Milestones.ElementAt(request.Index.Value);
                    milestone.IsCompleted = request.IsChecked;
                    milestone.UpdatedAt = now;

                    foreach (var section in milestone.Sections)
                    {
                        section.IsCompleted = request.IsChecked;
                        section.UpdatedAt = now;

                        foreach (var task in section.ToDoTasks)
                        {
                            task.IsCompleted = request.IsChecked;
                            task.UpdatedAt = now;
                        }
                    }

                    milestone.MilestoneProgress = CalculateMilestoneProgress(milestone); 
                    roadmap.OverallProgress = CalculateRoadmapProgress(roadmap);
                    if (roadmap.OverallProgress == 100) roadmap.IsCompleted = true;
                }
                else if (request.Type == "section" && request.Index.HasValue && request.ParentIndex.HasValue)
                {
                    var milestone = roadmap.Milestones.ElementAt(request.ParentIndex.Value);
                    var section = milestone.Sections.ElementAt(request.Index.Value);

                    section.IsCompleted = request.IsChecked;
                    section.UpdatedAt = now;

                    foreach (var task in section.ToDoTasks)
                    {
                        task.IsCompleted = request.IsChecked;
                        task.UpdatedAt = now;
                    }

                    milestone.IsCompleted = milestone.Sections.All(s => s.IsCompleted);
                    milestone.UpdatedAt = now;

                    milestone.MilestoneProgress = CalculateMilestoneProgress(milestone); 
                    roadmap.OverallProgress = CalculateRoadmapProgress(roadmap);
                    if (roadmap.OverallProgress == 100) roadmap.IsCompleted = true;
                }
                else if (request.Type == "task" && request.Index.HasValue && request.ParentIndex.HasValue && request.GrandParentIndex.HasValue)
                {
                    var milestone = roadmap.Milestones.ElementAt(request.GrandParentIndex.Value);
                    var section = milestone.Sections.ElementAt(request.ParentIndex.Value);
                    var task = section.ToDoTasks.ElementAt(request.Index.Value);

                    task.IsCompleted = request.IsChecked;
                    task.UpdatedAt = now;

                    section.IsCompleted = section.ToDoTasks.All(t => t.IsCompleted);
                    section.UpdatedAt = now;

                    milestone.IsCompleted = milestone.Sections.All(s => s.IsCompleted);
                    milestone.UpdatedAt = now;

                    milestone.MilestoneProgress = CalculateMilestoneProgress(milestone);
                    roadmap.OverallProgress = CalculateRoadmapProgress(roadmap);
                    if (roadmap.OverallProgress == 100) roadmap.IsCompleted = true;
                }

                roadmap.UpdatedAt = now;
                await _context.SaveChangesAsync(cancellationToken);
            }

            private static int CalculateMilestoneProgress(Milestone milestone)
            {
                var totalTasks = milestone.Sections.Sum(s => s.ToDoTasks.Count);
                var completedTasks = milestone.Sections.Sum(s => s.ToDoTasks.Count(t => t.IsCompleted));
                return totalTasks > 0 ? (int)((completedTasks / (double)totalTasks) * 100) : 0;
            }

            private static int CalculateRoadmapProgress(Roadmap roadmap)
            {
                var totalTasks = roadmap.Milestones.Sum(m => m.Sections.Sum(s => s.ToDoTasks.Count));
                var completedTasks = roadmap.Milestones.Sum(m => m.Sections.Sum(s => s.ToDoTasks.Count(t => t.IsCompleted)));
                return totalTasks > 0 ? (int)((completedTasks / (double)totalTasks) * 100) : 0;
            }
        }
    }
}
