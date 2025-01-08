using Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Serilog;
using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Text.Json;

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

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<RoadmapResponseDto> Handle(Query request, CancellationToken cancellationToken)
            {
                var roadmap = await _context.Roadmaps
                    .Include(r => r.Milestones)
                        .ThenInclude(m => m.Sections)
                            .ThenInclude(s => s.ToDoTasks)
                    .FirstOrDefaultAsync(r => r.RoadmapId == request.Id, cancellationToken);

                if (roadmap == null)
                {
                    throw new Exception("Roadmap not found");
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
                            Tasks = s.ToDoTasks.Select(t => new TaskResponseDto
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
                var traceId = Guid.NewGuid().ToString();
                var roadmapJson = JsonSerializer.Serialize(roadmap, new JsonSerializerOptions
                {
                    ReferenceHandler = ReferenceHandler.Preserve,
                });
                Log.Information("[{Timestamp:yyyy-MM-dd HH:mm:ss}] [INFO] [TraceId: {TraceId}] Get Roadmap: {Roadmap}",
                DateTime.UtcNow,
                traceId,
                roadmapJson);

                return response;
            }
        }
    }
}
