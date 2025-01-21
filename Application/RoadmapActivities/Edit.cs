using Domain.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

public class Edit
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
                throw new InvalidOperationException($"No Roadmap with Id '{request.Id}'.");

            if (request.Title != null) roadmap.Title = request.Title;
            if (request.Description != null) roadmap.Description = request.Description;

            if (request.Milestones != null)
            {
                foreach (var milestoneDto in request.Milestones)
                {
                    var milestone = roadmap.Milestones.FirstOrDefault(m => m.MilestoneId == milestoneDto.MilestoneId);

                    if (milestone != null && milestoneDto.IsDeleted)
                    {
                        milestone.IsDeleted = true;
                        milestone.UpdatedAt = DateTime.UtcNow;
                        continue;
                    }

                    if (milestone == null)
                    {
                        milestone = new Domain.Milestone
                        {
                            MilestoneId = milestoneDto.MilestoneId != Guid.Empty ? milestoneDto.MilestoneId : Guid.NewGuid(),
                            RoadmapId = roadmap.RoadmapId,
                            Name = milestoneDto.Name,
                            Description = milestoneDto.Description,
                            IsDeleted = milestoneDto.IsDeleted,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            Sections = new List<Domain.Section>()
                        };
                        _context.Milestones.Add(milestone);
                    }
                    else
                    {
                        if (milestoneDto.Name != null) milestone.Name = milestoneDto.Name;
                        if (milestoneDto.Description != null) milestone.Description = milestoneDto.Description;
                        milestone.UpdatedAt = DateTime.UtcNow;
                    }

                    foreach (var sectionDto in milestoneDto.Sections ?? new List<SectionResponseDto>())
                    {
                        var section = milestone.Sections.FirstOrDefault(s => s.SectionId == sectionDto.SectionId);

                        if (section != null && sectionDto.IsDeleted)
                        {
                            section.IsDeleted = true;
                            section.UpdatedAt = DateTime.UtcNow;
                            continue;
                        }

                        if (section == null)
                        {
                            section = new Domain.Section
                            {
                                SectionId = sectionDto.SectionId != Guid.Empty ? sectionDto.SectionId : Guid.NewGuid(),
                                MilestoneId = milestone.MilestoneId,
                                Name = sectionDto.Name,
                                Description = sectionDto.Description,
                                IsDeleted = sectionDto.IsDeleted,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow,
                                ToDoTasks = new List<Domain.ToDoTask>()
                            };
                            _context.Sections.Add(section);
                        }
                        else
                        {
                            if (sectionDto.Name != null) section.Name = sectionDto.Name;
                            if (sectionDto.Description != null) section.Description = sectionDto.Description;
                            section.UpdatedAt = DateTime.UtcNow;
                        }

                        foreach (var taskDto in sectionDto.Tasks ?? new List<TaskResponseDto>())
                        {
                            var task = section.ToDoTasks.FirstOrDefault(t => t.TaskId == taskDto.TaskId);

                            if (task != null && taskDto.IsDeleted)
                            {
                                task.IsDeleted = true;
                                task.UpdatedAt = DateTime.UtcNow;
                                continue;
                            }

                            if (task == null)
                            {
                                task = new Domain.ToDoTask
                                {
                                    TaskId = taskDto.TaskId != Guid.Empty ? taskDto.TaskId : Guid.NewGuid(),
                                    SectionId = section.SectionId,
                                    Name = taskDto.Name,
                                    DateStart = taskDto.DateStart,
                                    DateEnd = taskDto.DateEnd,
                                    IsCompleted = taskDto.IsCompleted,
                                    IsDeleted = taskDto.IsDeleted,
                                    CreatedAt = DateTime.UtcNow,
                                    UpdatedAt = DateTime.UtcNow
                                };
                                _context.ToDoTasks.Add(task);
                            }
                            else
                            {
                                if (taskDto.Name != null) task.Name = taskDto.Name;
                                if (taskDto.DateStart != default) task.DateStart = taskDto.DateStart;
                                if (taskDto.DateEnd != default) task.DateEnd = taskDto.DateEnd;
                                task.IsCompleted = taskDto.IsCompleted;
                                task.UpdatedAt = DateTime.UtcNow;
                            }
                        }
                    }
                }
            }

            roadmap.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
