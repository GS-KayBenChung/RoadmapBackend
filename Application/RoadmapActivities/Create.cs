using Domain;
using Domain.Dtos;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Persistence;

public class Create
{
    public class Command : IRequest
    {
        public CreateRoadmapDto RoadmapDto { get; set; }
    }

    public class Handler : IRequestHandler<Command>
    {
        private readonly DataContext _context;
        private readonly IValidator<CreateRoadmapDto> _validator;

        public Handler(DataContext context, IValidator<CreateRoadmapDto> validator)
        {
            _context = context;
            _validator = validator;
        }

        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            var traceId = Guid.NewGuid().ToString();
            using (Serilog.Context.LogContext.PushProperty("TraceId", traceId))
            {
                Log.Information("Processing roadmap creation...");

                var validationResult = await _validator.ValidateAsync(request.RoadmapDto, cancellationToken);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors
                        .ToDictionary(e => e.PropertyName, e => e.ErrorMessage);

                    Log.Warning("Validation failed: {Errors}", string.Join(", ", errors.Select(e => $"{e.Key}: {e.Value}")));
                    throw new ValidationException(new List<FluentValidation.Results.ValidationFailure>
                    {
                        new("Validation", string.Join(", ", errors.Select(e => e.Value)))
                    });
                }

                var userExists = await _context.UserRoadmap.AnyAsync(u => u.UserId == request.RoadmapDto.CreatedBy, cancellationToken);
                if (!userExists)
                {
                    Log.Warning("Validation failed: User with ID {UserId} does not exist in the database", request.RoadmapDto.CreatedBy);
                    throw new ValidationException(new List<FluentValidation.Results.ValidationFailure>
                    {
                        new("Validation", $"User with ID {request.RoadmapDto.CreatedBy} does not exist")
                    });
                }

                if (await _context.Roadmaps.AnyAsync(r => r.Title == request.RoadmapDto.Title && !r.IsDeleted, cancellationToken))
                {
                    Log.Warning("Validation failed: Roadmap with title '{Title}' already exists", request.RoadmapDto.Title);
                    throw new ValidationException(new List<FluentValidation.Results.ValidationFailure>
                    {
                        new("Validation", $"Roadmap with title '{request.RoadmapDto.Title}' already exists")
                    });
                }

                var roadmap = new Roadmap
                {
                    Title = request.RoadmapDto.Title,
                    Description = request.RoadmapDto.Description,
                    IsDraft = request.RoadmapDto.IsDraft ?? false,
                    CreatedBy = request.RoadmapDto.CreatedBy,
                    CreatedAt = DateTime.UtcNow,
                    OverallDuration = request.RoadmapDto.OverallDuration,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Roadmaps.Add(roadmap);
                await _context.SaveChangesAsync(cancellationToken);
                await Task.Delay(200, cancellationToken);

                if (!roadmap.IsDraft && request.RoadmapDto.Milestones?.Count > 0)
                {
                    await ProcessMilestonesAsync(request.RoadmapDto, roadmap, cancellationToken);
                }

                Log.Information("[{TraceId}] Roadmap '{Title}' created successfully", traceId, roadmap.Title);
            }
        }

        private async Task ProcessMilestonesAsync(CreateRoadmapDto roadmapDto, Roadmap roadmap, CancellationToken cancellationToken)
        {
            foreach (var milestoneDto in roadmapDto.Milestones ?? new List<CreateMilestoneDto>())
            {
                var milestone = new Milestone
                {
                    RoadmapId = roadmap.RoadmapId,
                    Name = milestoneDto.Name,
                    Description = milestoneDto.Description,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Milestones.Add(milestone);
                await _context.SaveChangesAsync(cancellationToken);
                await Task.Delay(200, cancellationToken);

                await ProcessSectionsAsync(milestoneDto, milestone, cancellationToken);
            }
        }

        private async Task ProcessSectionsAsync(CreateMilestoneDto milestoneDto, Milestone milestone, CancellationToken cancellationToken)
        {
            foreach (var sectionDto in milestoneDto.Sections ?? new List<CreateSectionDto>())
            {
                var section = new Section
                {
                    MilestoneId = milestone.MilestoneId,
                    Name = sectionDto.Name,
                    Description = sectionDto.Description,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Sections.Add(section);
                await _context.SaveChangesAsync(cancellationToken);
                await Task.Delay(200, cancellationToken);

                await ProcessTasksAsync(sectionDto, section, cancellationToken);
            }
        }

        private async Task ProcessTasksAsync(CreateSectionDto sectionDto, Section section, CancellationToken cancellationToken)
        {
            foreach (var taskDto in sectionDto.Tasks ?? new List<CreateTaskDto>())
            {
                var task = new ToDoTask
                {
                    SectionId = section.SectionId,
                    Name = taskDto.Name,
                    DateStart = taskDto.DateStart,
                    DateEnd = taskDto.DateEnd,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.ToDoTasks.Add(task);
                await _context.SaveChangesAsync(cancellationToken);
                await Task.Delay(200, cancellationToken);
            }
        }
    }
}
