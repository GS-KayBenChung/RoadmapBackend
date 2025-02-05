using Domain;
using Domain.Dtos;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Persistence;
using Serilog;
using System.Text.Json;
using System.Text.Json.Serialization;

public class Create
{
    //public class Command : IRequest<StatusDto>
    public class Command : IRequest
    {
        public RoadmapDto RoadmapDto { get; set; }
    }
    //public class Handler : IRequestHandler<Create.Command, StatusDto>
    public class Handler : IRequestHandler<Command>
    {
        private readonly DataContext _context;
        //private readonly IValidator<RoadmapDto> _validator;

        //public Handler(DataContext context, IValidator<RoadmapDto> validator)
        public Handler(DataContext context)

        {
            _context = context;
            //_validator = validator;
        }

        //public async Task<StatusDto> Handle(Create.Command request, CancellationToken cancellationToken)
        public async Task Handle(Create.Command request, CancellationToken cancellationToken)

        {

            var traceId = Guid.NewGuid().ToString();

            //var validationResult = await _validator.ValidateAsync(request.RoadmapDto, cancellationToken);
            //if (!validationResult.IsValid)
            //{
            //    Console.WriteLine($"CreatedBy Value: {request.RoadmapDto.CreatedBy}");

            //    var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            //    Log.Warning("[{TraceId}] Validation failed: {Errors}", traceId, errors);
            //    throw new ValidationException(errors);
            //}

            var existingRoadmap = await _context.Roadmaps.FirstOrDefaultAsync(
                r => r.Title == request.RoadmapDto.Title && !r.IsDeleted,
                cancellationToken);

            if (existingRoadmap != null)
            {
                Log.Warning("[{TraceId}] Roadmap with title '{Title}' already exists", traceId, request.RoadmapDto.Title);
                throw new Exception($"Roadmap with title '{request.RoadmapDto.Title}' already exists.");
            }

            var roadmap = new Roadmap
            {
                Title = request.RoadmapDto.Title,
                Description = request.RoadmapDto.Description,
                IsDraft = request.RoadmapDto.IsDraft,
                CreatedBy = request.RoadmapDto.CreatedBy,
                CreatedAt = request.RoadmapDto.CreatedAt,
                OverallDuration = request.RoadmapDto.OverallDuration,
                UpdatedAt = DateTime.UtcNow,
            };

                _context.Roadmaps.Add(roadmap);
                await _context.SaveChangesAsync(cancellationToken);

            if (request.RoadmapDto.Milestones != null && request.RoadmapDto.Milestones.Count != 0)
                {
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
                        await _context.SaveChangesAsync(cancellationToken);
                        await Task.Delay(200, cancellationToken);

                        if (milestoneDto.Sections != null && milestoneDto.Sections.Count != 0)
                        {
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
                                await _context.SaveChangesAsync(cancellationToken);
                                await Task.Delay(200, cancellationToken);

                                if (sectionDto.Tasks != null && sectionDto.Tasks.Count != 0)
                                {
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
                                            await _context.SaveChangesAsync(cancellationToken);
                                            await Task.Delay(200, cancellationToken);
                                        }
                                }
                            }
                        }
                    }
            }
            await _context.SaveChangesAsync(cancellationToken);
            Log.Information("[{TraceId}] Created Roadmap: {Roadmap}", traceId, roadmap);
        }
    }
}
