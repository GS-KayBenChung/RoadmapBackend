using Application.DTOs;
using Domain;
using MediatR;
using Persistence;

public class Create
{
    public class Command : IRequest
    {
        public RoadmapDto RoadmapDto { get; set; }
    }

    public class Handler : IRequestHandler<Create.Command>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task Handle(Create.Command request, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrEmpty(request.RoadmapDto.Title) || request.RoadmapDto.Title.Length > 50)
                {
                    throw new ArgumentException("Roadmap title must be between 1 and 50 characters.");
                }

                if (string.IsNullOrEmpty(request.RoadmapDto.Description) || request.RoadmapDto.Description.Length > 100)
                {
                    throw new ArgumentException("Roadmap description must be between 1 and 100 characters.");
                }

                var roadmap = new Roadmap
                {
                    Title = request.RoadmapDto.Title,
                    Description = request.RoadmapDto.Description,
                    IsDraft = request.RoadmapDto.IsDraft,
                    CreatedBy = request.RoadmapDto.CreatedBy,
                    CreatedAt = request.RoadmapDto.CreatedAt,
                    UpdatedAt = DateTime.UtcNow,
                };

                _context.Roadmaps.Add(roadmap);

                foreach (var milestoneDto in request.RoadmapDto.Milestones)
                {
                    var milestone = new Milestone
                    {
                        RoadmapId = roadmap.RoadmapId,
                        Name = milestoneDto.Name,
                        Description = milestoneDto.Description,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                    };
                    _context.Milestones.Add(milestone);

                    foreach (var sectionDto in milestoneDto.Sections)
                    {
                        var section = new Section
                        {
                            MilestoneId = milestone.MilestoneId,
                            Name = sectionDto.Name,
                            Description = sectionDto.Description,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                        };
                        _context.Sections.Add(section);

                        foreach (var taskDto in sectionDto.Tasks)
                        {
                            var task = new ToDoTask
                            {
                                SectionId = section.SectionId,
                                Name = taskDto.Name,
                                DateStart = taskDto.DateStart,
                                DateEnd = taskDto.DateEnd,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow,
                            };
                            _context.ToDoTasks.Add(task);
                        }
                    }
                }

                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (ArgumentException ex)
            {
                throw new ApplicationException($"Validation error: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An unexpected error occurred while creating the roadmap.");
            }
        }
    }
}
