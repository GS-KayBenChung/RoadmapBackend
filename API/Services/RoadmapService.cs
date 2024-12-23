using Application.DTOs;
using Domain;
using Persistence;

namespace API.Services
{
    public class RoadmapService
    {
        private readonly DataContext _context;

        public RoadmapService(DataContext context)
        {
            _context = context;
        }

        public async Task<Roadmap> CreateRoadmapAsync(RoadmapDto roadmapDto)
        {
            var roadmap = new Roadmap
            {
                RoadmapId = Guid.NewGuid(),
                Title = roadmapDto.Title,
                Description = roadmapDto.Description,
                CreatedBy = roadmapDto.CreatedBy,
                CreatedAt = roadmapDto.CreatedAt, 
                UpdatedAt = DateTime.UtcNow, 
                IsCompleted = false,
                IsDeleted = false,
                IsDraft = roadmapDto.IsDraft,
            };

            foreach (var milestoneDto in roadmapDto.Milestones)
            {
                var milestone = new Milestone
                {
                    MilestoneId = Guid.NewGuid(),
                    Name = milestoneDto.Name,
                    Description = milestoneDto.Description,
                    RoadmapId = roadmap.RoadmapId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow, 
                    IsCompleted = false,
                    IsDeleted = false
                };

                foreach (var sectionDto in milestoneDto.Sections)
                {
                    var section = new Section
                    {
                        SectionId = Guid.NewGuid(),
                        Name = sectionDto.Name,
                        Description = sectionDto.Description,
                        MilestoneId = milestone.MilestoneId,
                        CreatedAt = DateTime.UtcNow, 
                        UpdatedAt = DateTime.UtcNow, 
                        IsCompleted = false,
                        IsDeleted = false
                    };

                    foreach (var taskDto in sectionDto.Tasks)
                    {
                        var task = new ToDoTask
                        {
                            TaskId = Guid.NewGuid(),
                            Name = taskDto.Name,
                            DateStart = taskDto.DateStart,
                            DateEnd = taskDto.DateEnd,
                            SectionId = section.SectionId,
                            CreatedAt = DateTime.UtcNow, 
                            UpdatedAt = DateTime.UtcNow, 
                            IsCompleted = false,
                            IsDeleted = false
                        };

                        section.ToDoTasks.Add(task);
                    }

                    milestone.Sections.Add(section);
                }

                roadmap.Milestones.Add(milestone);
            }

            _context.Roadmaps.Add(roadmap);
            await _context.SaveChangesAsync();

            return roadmap;
        }
    }
}
