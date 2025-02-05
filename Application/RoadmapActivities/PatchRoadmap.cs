using Domain;
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

                var roadmap = await _context.Roadmaps.FirstOrDefaultAsync(r => r.RoadmapId == request.RoadmapId, cancellationToken);
                if (roadmap == null)
                {
                    throw new InvalidOperationException($"No Roadmap with ID '{request.RoadmapId}' found.");
                }

                if (request.UpdateDto.Roadmap != null)
                {
                    roadmap.Title = request.UpdateDto.Roadmap.Title ?? roadmap.Title;
                    roadmap.Description = request.UpdateDto.Roadmap.Description ?? roadmap.Description;
                    roadmap.UpdatedAt = DateTime.UtcNow;
                }

                if (request.UpdateDto.Milestones?.Any() == true)
                {
                    var milestoneIds = request.UpdateDto.Milestones.Select(m => m.MilestoneId).ToList();
                    var milestones = await _context.Milestones.Where(m => milestoneIds.Contains(m.MilestoneId)).ToListAsync(cancellationToken);

                    foreach (var milestone in milestones)
                    {
                        var update = request.UpdateDto.Milestones.FirstOrDefault(m => m.MilestoneId == milestone.MilestoneId);
                        if (update.IsDeleted)
                        {
                            milestone.IsDeleted = true;
                        }
                        else
                        {
                            milestone.Name = update.Name ?? milestone.Name;
                            milestone.Description = update.Description ?? milestone.Description;
                        }
                        milestone.UpdatedAt = DateTime.UtcNow;
                    }

                    var existingMilestoneIds = milestones.Select(m => m.MilestoneId).ToList();
                    var newMilestones = request.UpdateDto.Milestones.Where(m => !existingMilestoneIds.Contains(m.MilestoneId) && !m.IsDeleted).ToList();

                    foreach (var newMilestone in newMilestones)
                    {
                        var milestone = new Milestone
                        {
                            MilestoneId = newMilestone.MilestoneId,
                            Name = newMilestone.Name,
                            Description = newMilestone.Description,
                            RoadmapId = request.RoadmapId,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            IsDeleted = false
                        };
                        _context.Milestones.Add(milestone);
                    }
                    await _context.SaveChangesAsync(cancellationToken);
                }

                if (request.UpdateDto.Sections?.Any() == true)
                {
                    var sectionIds = request.UpdateDto.Sections.Select(s => s.SectionId).ToList();
                    var sections = await _context.Sections.Where(s => sectionIds.Contains(s.SectionId)).ToListAsync(cancellationToken);

                    foreach (var section in sections)
                    {
                        var update = request.UpdateDto.Sections.FirstOrDefault(s => s.SectionId == section.SectionId);
                        if (update.IsDeleted)
                        {
                            section.IsDeleted = true;
                        }
                        else
                        {
                            section.Name = update.Name ?? section.Name;
                            section.Description = update.Description ?? section.Description;
                        }
                        section.UpdatedAt = DateTime.UtcNow;
                    }

                    var existingSectionIds = sections.Select(s => s.SectionId).ToList();
                    var newSections = request.UpdateDto.Sections.Where(s => !existingSectionIds.Contains(s.SectionId) && !s.IsDeleted).ToList();

                    foreach (var newSection in newSections)
                    {
                        var section = new Section
                        {
                            SectionId = newSection.SectionId,
                            MilestoneId = newSection.MilestoneId,
                            Name = newSection.Name,
                            Description = newSection.Description,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            IsDeleted = false 
                        };
                        _context.Sections.Add(section);
                    }
                    await _context.SaveChangesAsync(cancellationToken);
                }

                if (request.UpdateDto.Tasks?.Any() == true)
                {
                    var taskIds = request.UpdateDto.Tasks.Select(t => t.TaskId).ToList();
                    var tasks = await _context.ToDoTasks.Where(t => taskIds.Contains(t.TaskId)).ToListAsync(cancellationToken);

                    foreach (var task in tasks)
                    {
                        var update = request.UpdateDto.Tasks.FirstOrDefault(t => t.TaskId == task.TaskId);
                        if (update.IsDeleted)
                        {
                            task.IsDeleted = true; 
                        }
                        else
                        {
                            task.Name = update.Name ?? task.Name;
                            task.DateStart = update.DateStart ?? task.DateStart;
                            task.DateEnd = update.DateEnd ?? task.DateEnd;
                        }
                        task.UpdatedAt = DateTime.UtcNow;
                    }

                    var existingTaskIds = tasks.Select(t => t.TaskId).ToList();
                    var newTasks = request.UpdateDto.Tasks.Where(t => !existingTaskIds.Contains(t.TaskId) && !t.IsDeleted).ToList();

                    foreach (var newTask in newTasks)
                    {
                        var task = new ToDoTask
                        {
                            SectionId = newTask.SectionId,
                            TaskId = newTask.TaskId,
                            Name = newTask.Name,
                            DateStart = (DateTime)newTask.DateStart,
                            DateEnd = (DateTime)newTask.DateEnd,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            IsDeleted = false 
                        };
                        _context.ToDoTasks.Add(task);
                    }
                    await _context.SaveChangesAsync(cancellationToken);
                }

                var result = await _context.SaveChangesAsync(cancellationToken);
               
            }

        }
    }
}
