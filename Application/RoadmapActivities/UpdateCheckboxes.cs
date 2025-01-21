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
            public Guid? MilestoneId { get; set; }
            public Guid? SectionId { get; set; }
            public Guid? TaskId { get; set; }
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
                else if (request.Type == "milestone" && request.MilestoneId.HasValue)
                {
                    var milestone = await _context.Milestones
                        .Include(m => m.Sections)
                            .ThenInclude(s => s.ToDoTasks)
                        .FirstOrDefaultAsync(m => m.MilestoneId == request.MilestoneId.Value, cancellationToken);

                    if (milestone == null)
                        throw new Exception("Milestone not found");

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
                else if (request.Type == "section" && request.SectionId.HasValue && request.MilestoneId.HasValue)
                {
                    var milestone = await _context.Milestones
                        .Include(m => m.Sections)
                            .ThenInclude(s => s.ToDoTasks)
                        .FirstOrDefaultAsync(m => m.MilestoneId == request.MilestoneId.Value, cancellationToken);

                    if (milestone == null)
                        throw new Exception("Milestone not found");

                    var section = milestone.Sections.FirstOrDefault(s => s.SectionId == request.SectionId.Value);
                    if (section == null)
                        throw new Exception("Section not found");

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
                else if (request.Type == "task" && request.TaskId.HasValue && request.SectionId.HasValue && request.MilestoneId.HasValue)
                {
                    var milestone = await _context.Milestones
                        .Include(m => m.Sections)
                            .ThenInclude(s => s.ToDoTasks)
                        .FirstOrDefaultAsync(m => m.MilestoneId == request.MilestoneId.Value, cancellationToken);

                    if (milestone == null)
                        throw new Exception("Milestone not found");

                    var section = milestone.Sections.FirstOrDefault(s => s.SectionId == request.SectionId.Value);
                    if (section == null)
                        throw new Exception("Section not found");

                    var task = section.ToDoTasks.FirstOrDefault(t => t.TaskId == request.TaskId.Value);
                    if (task == null)
                        throw new Exception("Task not found");

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
