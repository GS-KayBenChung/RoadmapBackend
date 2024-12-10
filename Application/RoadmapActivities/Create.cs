//using Domain;
//using MediatR;
//using Persistence;

//namespace Application.RoadmapActivities
//{
//    public class Create
//    {
//        public class Command: IRequest
//        {
//            public Roadmap Roadmap { get; set; }

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
//                _context.Roadmaps.Add(request.Roadmap);
//                await _context.SaveChangesAsync();
//            }

//        }
//    }
//}
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
            // Ensure CreatedAt and UpdatedAt are assigned explicitly.
            var roadmap = new Roadmap
            {
                Title = request.RoadmapDto.Title,
                Description = request.RoadmapDto.Description,
                CreatedBy = request.RoadmapDto.CreatedBy,
                CreatedAt = request.RoadmapDto.CreatedAt, // Use the value from DTO
                UpdatedAt = DateTime.UtcNow, // Use current UTC time for UpdatedAt
            };

            _context.Roadmaps.Add(roadmap);

            foreach (var milestoneDto in request.RoadmapDto.Milestones)
            {
                var milestone = new Milestone
                {
                    RoadmapId = roadmap.RoadmapId,
                    Name = milestoneDto.Name,
                    Description = milestoneDto.Description,
                    CreatedAt = DateTime.UtcNow, // Set to current UTC time
                    UpdatedAt = DateTime.UtcNow, // Set to current UTC time
                };
                _context.Milestones.Add(milestone);

                foreach (var sectionDto in milestoneDto.Sections)
                {
                    var section = new Section
                    {
                        MilestoneId = milestone.MilestoneId,
                        Name = sectionDto.Name,
                        Description = sectionDto.Description,
                        CreatedAt = DateTime.UtcNow, // Set to current UTC time
                        UpdatedAt = DateTime.UtcNow, // Set to current UTC time
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
                            CreatedAt = DateTime.UtcNow, // Set to current UTC time
                            UpdatedAt = DateTime.UtcNow, // Set to current UTC time
                        };
                        _context.ToDoTasks.Add(task);
                    }
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
