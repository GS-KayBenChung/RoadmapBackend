using Application.Validator;
using Domain;
using Domain.Dtos;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Serilog;

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
            private readonly IValidationService _validationService;

            public Handler(DataContext context, IValidationService validationService)
            {
                _context = context;
                _validationService = validationService;
            }

            public async Task Handle(Command request, CancellationToken cancellationToken)
            {
                await _validationService.ValidateAsync(request, cancellationToken);

                var traceId = Guid.NewGuid().ToString();
                Log.Information("Started updating roadmap with ID: {RoadmapId}", request.RoadmapId);

                var roadmap = await _context.Roadmaps
                    .Include(r => r.Milestones)
                        .ThenInclude(m => m.Sections)
                            .ThenInclude(s => s.ToDoTasks)
                    .FirstOrDefaultAsync(r => r.RoadmapId == request.RoadmapId, cancellationToken);

                if (roadmap == null)
                {
                    Log.Warning("Roadmap with ID {RoadmapId} not found.", request.RoadmapId);
                    throw new ValidationException(new List<FluentValidation.Results.ValidationFailure>
                    {
                        new("RoadmapId", "Roadmap not found.")
                    });
                }

                if (roadmap.IsDeleted)
                {
                    Log.Warning(" Roadmap with ID {RoadmapId} is deleted and cannot be updated.", request.RoadmapId);
                    throw new ValidationException(new List<FluentValidation.Results.ValidationFailure>
                    {
                        new("RoadmapId", "This roadmap has been deleted and cannot be updated.")
                    });
                }

                var updateDto = request.UpdateDto ?? new RoadmapUpdateDto();
                updateDto.Roadmap ??= new RoadmapPatchDto();
                updateDto.Milestones ??= new List<MilestonePatchDto>();
                updateDto.Sections ??= new List<SectionPatchDto>();
                updateDto.Tasks ??= new List<TaskPatchDto>();

                if (!string.IsNullOrWhiteSpace(updateDto.Roadmap.Title) || !string.IsNullOrWhiteSpace(updateDto.Roadmap.Description))
                {
                    roadmap.Title = updateDto.Roadmap.Title ?? roadmap.Title;
                    roadmap.Description = updateDto.Roadmap.Description ?? roadmap.Description;
                    roadmap.UpdatedAt = DateTime.UtcNow;
                }

                //  Update Milestones
                if (updateDto.Milestones.Any())
                {
                    var existingMilestones = roadmap.Milestones.ToDictionary(m => m.MilestoneId);

                    foreach (var update in updateDto.Milestones)
                    {
                        if (update.MilestoneId == Guid.Empty)
                        {
                            throw new ValidationException($"Milestone '{update.Name}' is missing a valid milestoneId.");
                        }

                        if (existingMilestones.TryGetValue(update.MilestoneId, out var milestone))
                        {
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
                        else if (!update.IsDeleted)
                        {
                            _context.Milestones.Add(new Milestone
                            {
                                MilestoneId = update.MilestoneId,
                                RoadmapId = roadmap.RoadmapId,
                                Name = update.Name,
                                Description = update.Description,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow,
                                IsDeleted = false
                            });
                        }
                    }
                }

                //  Update Sections
                if (updateDto.Sections.Any())
                {
                    var existingSections = roadmap.Milestones.SelectMany(m => m.Sections).ToDictionary(s => s.SectionId);

                    foreach (var update in updateDto.Sections)
                    {
                        if (update.SectionId == Guid.Empty || update.MilestoneId == Guid.Empty)
                        {
                            throw new ValidationException($"Section '{update.Name}' is missing a valid SectionId or MilestoneId.");
                        }

                        if (existingSections.TryGetValue(update.SectionId, out var section))
                        {
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
                        else if (!update.IsDeleted)
                        {
                            _context.Sections.Add(new Section
                            {
                                SectionId = update.SectionId,
                                MilestoneId = update.MilestoneId,
                                Name = update.Name,
                                Description = update.Description,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow,
                                IsDeleted = false
                            });
                        }
                    }
                }

                //  Update Tasks
                if (updateDto.Tasks.Any())
                {
                    var existingTasks = roadmap.Milestones.SelectMany(m => m.Sections)
                                                          .SelectMany(s => s.ToDoTasks)
                                                          .ToDictionary(t => t.TaskId);

                    foreach (var update in updateDto.Tasks)
                    {
                        if (update.TaskId == Guid.Empty || update.SectionId == Guid.Empty || update.MilestoneId == Guid.Empty)
                        {
                            throw new ValidationException($"Task '{update.Name}' is missing a valid TaskId, SectionId, or MilestoneId.");
                        }

                        if (existingTasks.TryGetValue(update.TaskId, out var task))
                        {
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
                        else if (!update.IsDeleted)
                        {
                            if (string.IsNullOrWhiteSpace(update.Name) || update.DateStart == null || update.DateEnd == null)
                            {
                                throw new ValidationException($"New task '{update.TaskId}' must have a name, start date, and end date.");
                            }

                            if (update.DateStart >= update.DateEnd)
                            {
                                throw new ValidationException($"New task '{update.TaskId}' has an invalid date range. Start date must be before end date.");
                            }

                            _context.ToDoTasks.Add(new ToDoTask
                            {
                                TaskId = update.TaskId,
                                SectionId = update.SectionId,
                                Name = update.Name,
                                DateStart = update.DateStart.Value,
                                DateEnd = update.DateEnd.Value,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow,
                                IsDeleted = false
                            });
                        }
                    }
                }

                await _context.SaveChangesAsync(cancellationToken);
                Log.Information("Successfully updated roadmap with ID {RoadmapId}", request.RoadmapId);
            }
        }
    }
}
