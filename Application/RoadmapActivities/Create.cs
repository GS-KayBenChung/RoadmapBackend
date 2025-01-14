
using Application.DTOs;
using Domain;
using Domain.Dtos;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Serilog;
using Serilog.Context;
using System.Text.Json;
using System.Text.Json.Serialization;

public class Create
{
    public class Command : IRequest<StatusDto>
    {
        public RoadmapDto RoadmapDto { get; set; }
    }
    public class Handler : IRequestHandler<Create.Command, StatusDto>
    {
        private readonly DataContext _context;
        private readonly IValidator<RoadmapDto> _validator;

        public Handler(DataContext context, IValidator<RoadmapDto> validator)
        {
            _context = context;
            _validator = validator;
        }

        public async Task<StatusDto> Handle(Create.Command request, CancellationToken cancellationToken)
        {

            var traceId = Guid.NewGuid().ToString();

            var validationResult = await _validator.ValidateAsync(request.RoadmapDto, cancellationToken);

            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));
                Log.Error("[{Timestamp:yyyy-MM-dd HH:mm:ss}] [ERROR] [TraceId: {TraceId}] Validation failed: {Errors}", DateTime.UtcNow, traceId, errors);
                throw new ApplicationException("Validation failed: " + string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
            }

            var existingRoadmap = await _context.Roadmaps.FirstOrDefaultAsync(r => r.Title == request.RoadmapDto.Title && !r.IsDeleted, cancellationToken);

            if (existingRoadmap != null)
            {
                return new StatusDto
                {
                    err = $"Cannot create roadmap because roadmap with title '{request.RoadmapDto.Title}' already exists in the database.",
                    status = "401"
                };
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
                                    }
                                }
                            }
                        }
                    }

                    Log.Information("[{Timestamp:yyyy-MM-dd HH:mm:ss}] [INFO] [TraceId: {TraceId}] Create Roadmap: {Roadmap}",
                    DateTime.UtcNow,
                    traceId,
                    JsonSerializer.Serialize(roadmap, new JsonSerializerOptions
                    {
                        ReferenceHandler = ReferenceHandler.Preserve
                    }));

                }
                await _context.SaveChangesAsync(cancellationToken);
            return new StatusDto { err = "-1", status = "alsjdfajklsdf" };
        }
    }

}
