using Domain.Dtos;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Serilog;

using Application.Validator;

namespace Application.RoadmapActivities
{
    public class GetDetails
    {
        public class Query : IRequest<RoadmapResponseDto>
        {
            public Guid Id { get; set; }
        }

        public class Handler : IRequestHandler<Query, RoadmapResponseDto>
        {
            private readonly DataContext _context;
            private readonly IValidationService _validationService;

            public Handler(DataContext context, IValidationService validationService)
            {
                _context = context;
                _validationService = validationService;
            }

            public async Task<RoadmapResponseDto> Handle(Query request, CancellationToken cancellationToken)
            {
                await _validationService.ValidateAsync(request, cancellationToken);

                var traceId = Guid.NewGuid().ToString();

                var roadmap = await _context.Roadmaps
                    .Include(r => r.Milestones.Where(m => !m.IsDeleted)
                    .OrderBy(m => m.CreatedAt) 
                    )
                    .ThenInclude(m => m.Sections.Where(s => !s.IsDeleted)
                        .OrderBy(s => s.CreatedAt) 
                    )
                        .ThenInclude(s => s.ToDoTasks.Where(t => !t.IsDeleted)
                            .OrderBy(t => t.DateStart).ThenBy(t => t.TaskId) 
                        )
                .AsSplitQuery()
                .FirstOrDefaultAsync(r => r.RoadmapId == request.Id, cancellationToken);

                //.Include(r => r.Milestones.Where(m => !m.IsDeleted))
                //    .ThenInclude(m => m.Sections.Where(s => !s.IsDeleted))
                //        .ThenInclude(s => s.ToDoTasks.Where(t => !t.IsDeleted))
                //.AsSplitQuery()
                //.FirstOrDefaultAsync(r => r.RoadmapId == request.Id, cancellationToken);


                if (roadmap == null)
                {
                    Log.Warning("[{TraceId}] Roadmap with ID {RoadmapId} not found.", traceId, request.Id);
                    throw new ValidationException(new List<FluentValidation.Results.ValidationFailure>
                    {
                        new("RoadmapId", "Roadmap not found.")
                    });
                }

                if (roadmap.IsDeleted)
                {
                    Log.Warning("[{TraceId}] Attempted to retrieve deleted roadmap {RoadmapId}.", traceId, request.Id);
                    throw new ValidationException(new List<FluentValidation.Results.ValidationFailure>
                    {
                        new("RoadmapId", "This roadmap has been deleted and cannot be retrieved.")
                    });
                }

                var response = new RoadmapResponseDto
                {
                    RoadmapId = roadmap.RoadmapId,
                    Title = roadmap.Title,
                    Description = roadmap.Description,
                    CreatedBy = roadmap.CreatedBy,
                    OverallProgress = roadmap.OverallProgress,
                    OverallDuration = roadmap.OverallDuration,
                    IsCompleted = roadmap.IsCompleted,
                    IsDraft = roadmap.IsDraft,
                    IsDeleted = roadmap.IsDeleted,
                    CreatedAt = roadmap.CreatedAt,
                    UpdatedAt = roadmap.UpdatedAt,
                    Milestones = roadmap.Milestones.Select(m => new MilestoneResponseDto
                    {
                        MilestoneId = m.MilestoneId,
                        RoadmapId = m.RoadmapId,
                        Name = m.Name,
                        Description = m.Description,
                        MilestoneProgress = m.MilestoneProgress,
                        IsCompleted = m.IsCompleted,
                        Sections = m.Sections.Select(s => new SectionResponseDto
                        {
                            SectionId = s.SectionId,
                            MilestoneId = s.MilestoneId,
                            Name = s.Name,
                            Description = s.Description,
                            IsCompleted = s.IsCompleted,
                            Tasks = s.ToDoTasks
                            .OrderBy(t => t.DateStart)
                            .ThenBy(t => t.TaskId)
                            .Select(t => new TaskResponseDto
                            {
                                TaskId = t.TaskId,
                                SectionId = t.SectionId,
                                Name = t.Name,
                                DateStart = t.DateStart,
                                DateEnd = t.DateEnd,
                                IsCompleted = t.IsCompleted
                            }).ToList()
                        }).ToList()
                    }).ToList()
                };

                Log.Information("Roadmap details retrieved successfully: {RoadmapId}", roadmap.RoadmapId);

                return response;
            }
        }
    }
}
