//using System.Text.Json.Serialization;
//using System.Text.Json;
//using Application.DTOs;
//using MediatR;
//using Microsoft.EntityFrameworkCore;
//using Persistence;
//using Serilog;

//namespace Application.RoadmapActivities
//{
//    public class UpdateRoadmap
//    {
//        public class Command : IRequest
//        {
//            public Guid Id { get; set; }
//            public string Title { get; set; }
//            public string Description { get; set; }
//            public bool IsDraft { get; set; }
//            public List<MilestoneResponseDto> Milestones { get; set; }
//        }

//        public class Handler : IRequestHandler<Command>
//        {
//            private readonly DataContext _context;

//            public Handler(DataContext context)
//            {
//                _context = context;
//            }

//            public async Task Handle(Command request, CancellationToken cancellationToken)
//            {
//                var roadmap = await _context.Roadmaps
//                    .Include(r => r.Milestones)
//                        .ThenInclude(m => m.Sections)
//                            .ThenInclude(s => s.ToDoTasks)
//                    .FirstOrDefaultAsync(r => r.RoadmapId == request.Id, cancellationToken);

//                if (roadmap == null)
//                    throw new InvalidOperationException($"No Roadmap with Id '{request.Id}'.");

//                roadmap.Title = request.Title;
//                roadmap.Description = request.Description;
//                roadmap.IsDraft = request.IsDraft;
//                roadmap.UpdatedAt = DateTime.UtcNow;

//                foreach (var milestone in roadmap.Milestones)
//                {
//                    var matchingMilestone = request.Milestones.FirstOrDefault(m => m.MilestoneId == milestone.MilestoneId);

//                    if (matchingMilestone != null && matchingMilestone.IsDeleted)
//                    {
//                        milestone.IsDeleted = true;
//                        milestone.UpdatedAt = DateTime.UtcNow;
//                    }
//                }

//                foreach (var milestoneDto in request.Milestones)
//                {
//                    var milestone = roadmap.Milestones.FirstOrDefault(m => m.MilestoneId == milestoneDto.MilestoneId);

//                    if (milestone == null)
//                    {
//                        milestone = new Domain.Milestone
//                        {
//                            MilestoneId = milestoneDto.MilestoneId != Guid.Empty ? milestoneDto.MilestoneId : Guid.NewGuid(),
//                            RoadmapId = roadmap.RoadmapId,
//                            Name = milestoneDto.Name,
//                            Description = milestoneDto.Description,
//                            IsDeleted = milestoneDto.IsDeleted,
//                            CreatedAt = DateTime.UtcNow,
//                            UpdatedAt = DateTime.UtcNow,
//                            Sections = new List<Domain.Section>()
//                        };
//                        _context.Milestones.Add(milestone);
//                    }
//                    else
//                    {
//                        milestone.Name = milestoneDto.Name;
//                        milestone.Description = milestoneDto.Description;
//                        milestone.IsDeleted = milestoneDto.IsDeleted;
//                        milestone.UpdatedAt = DateTime.UtcNow;
//                    }

//                    foreach (var section in milestone.Sections)
//                    {
//                        var matchingSection = milestoneDto.Sections.FirstOrDefault(s => s.SectionId == section.SectionId);

//                        if (matchingSection != null && matchingSection.IsDeleted)
//                        {
//                            section.IsDeleted = true;
//                            section.UpdatedAt = DateTime.UtcNow;
//                        }
//                    }

//                    foreach (var sectionDto in milestoneDto.Sections)
//                    {
//                        var section = milestone.Sections.FirstOrDefault(s => s.SectionId == sectionDto.SectionId);

//                        if (section == null)
//                        {
//                            section = new Domain.Section
//                            {
//                                SectionId = sectionDto.SectionId != Guid.Empty ? sectionDto.SectionId : Guid.NewGuid(),
//                                MilestoneId = milestone.MilestoneId,
//                                Name = sectionDto.Name,
//                                Description = sectionDto.Description,
//                                IsDeleted = sectionDto.IsDeleted,
//                                CreatedAt = DateTime.UtcNow,
//                                UpdatedAt = DateTime.UtcNow,
//                                ToDoTasks = new List<Domain.ToDoTask>()
//                            };
//                            _context.Sections.Add(section);
//                        }
//                        else
//                        {
//                            section.Name = sectionDto.Name;
//                            section.Description = sectionDto.Description;
//                            section.IsDeleted = sectionDto.IsDeleted;
//                            section.UpdatedAt = DateTime.UtcNow;
//                        }

//                        foreach (var task in section.ToDoTasks)
//                        {
//                            var matchingTask = sectionDto.Tasks.FirstOrDefault(t => t.TaskId == task.TaskId);

//                            if (matchingTask != null && matchingTask.IsDeleted)
//                            {
//                                task.IsDeleted = true;
//                                task.UpdatedAt = DateTime.UtcNow;
//                            }
//                        }

//                        foreach (var taskDto in sectionDto.Tasks)
//                        {
//                            var task = section.ToDoTasks.FirstOrDefault(t => t.TaskId == taskDto.TaskId);

//                            if (task == null)
//                            {
//                                task = new Domain.ToDoTask
//                                {
//                                    TaskId = taskDto.TaskId != Guid.Empty ? taskDto.TaskId : Guid.NewGuid(),
//                                    SectionId = section.SectionId,
//                                    Name = taskDto.Name,
//                                    DateStart = taskDto.DateStart,
//                                    DateEnd = taskDto.DateEnd,
//                                    IsCompleted = taskDto.IsCompleted,
//                                    IsDeleted = taskDto.IsDeleted,
//                                    CreatedAt = DateTime.UtcNow,
//                                    UpdatedAt = DateTime.UtcNow
//                                };
//                                _context.ToDoTasks.Add(task);
//                            }
//                            else
//                            {
//                                task.Name = taskDto.Name;
//                                task.DateStart = taskDto.DateStart;
//                                task.DateEnd = taskDto.DateEnd;
//                                task.IsCompleted = taskDto.IsCompleted;
//                                task.IsDeleted = taskDto.IsDeleted;
//                                task.UpdatedAt = DateTime.UtcNow;
//                            }
//                        }
//                    }
//                }
//                var traceId = Guid.NewGuid().ToString();
//                var roadmapJson = JsonSerializer.Serialize(roadmap, new JsonSerializerOptions
//                {
//                    ReferenceHandler = ReferenceHandler.Preserve,
//                });
//                Log.Information("[{Timestamp:yyyy-MM-dd HH:mm:ss}] [INFO] [TraceId: {TraceId}] Edited Roadmap: {Roadmap}",
//                DateTime.UtcNow,
//                traceId,
//                roadmapJson);

//                try
//                {
//                    var success = await _context.SaveChangesAsync(cancellationToken);
//                    if (success <= 0) throw new Exception("Failed to update roadmap");
//                }
//                catch (DbUpdateException ex)
//                {
//                    throw new Exception("Database update failed.", ex);
//                }
//                catch (Exception ex)
//                {
//                    throw new Exception("Error.", ex);
//                    throw;
//                }
//            }


//        }
//    }
//}
using Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

public class Edit
{
    public class Command : IRequest
    {
        public Guid Id { get; set; }  // Roadmap ID
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
