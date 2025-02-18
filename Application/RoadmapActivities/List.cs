using Application.Validator;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Serilog;
using System.Linq.Dynamic.Core;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Application.RoadmapActivities
{
    public class List
    {
        public class Query : IRequest<PaginatedRoadmapResult<Roadmap>>
        {
            public string Filter { get; set; }
            public string Search { get; set; }
            public DateTime? CreatedAfter { get; set; }
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
            public string SortBy { get; set; }
            public int Asc { get; set; }
        }
        public class Handler : IRequestHandler<Query, PaginatedRoadmapResult<Roadmap>>
        {
            private readonly DataContext _context;
            private readonly IValidationService _validationService;

            public Handler(DataContext context, IValidationService validationService)
            {
                _context = context;
                _validationService = validationService;
            }

            public async Task<PaginatedRoadmapResult<Roadmap>> Handle(Query request, CancellationToken cancellationToken)
            {
                await _validationService.ValidateAsync(request, cancellationToken);

                var traceId = Guid.NewGuid().ToString();

                var query = _context.Roadmaps
                    .Where(r => !r.IsDeleted)
                    .AsNoTracking()
                    .AsQueryable();

                if (request.CreatedAfter.HasValue)
                {
                    var startOfDay = request.CreatedAfter.Value.Date;
                    var endOfDay = startOfDay.AddDays(1).AddTicks(-1);
                    query = query.Where(r => r.CreatedAt >= startOfDay);
                }

                if (!string.IsNullOrEmpty(request.Filter))
                {
                    query = request.Filter.ToLower() switch
                    {
                        "draft" => query.Where(r => r.IsDraft),
                        "published" => query.Where(r => !r.IsDraft),
                        "completed" => query.Where(r => r.IsCompleted),
                        "neardue" => query.Where(r => !r.IsDraft &&
                            r.Milestones.SelectMany(m => m.Sections)
                                        .SelectMany(s => s.ToDoTasks)
                                        .Where(t => !t.IsDeleted)
                                        .Max(t => (DateTime?)t.DateEnd) <= DateTime.UtcNow.AddDays(7) &&
                            r.Milestones.SelectMany(m => m.Sections)
                                        .SelectMany(s => s.ToDoTasks)
                                        .Where(t => !t.IsDeleted)
                                        .Max(t => (DateTime?)t.DateEnd) > DateTime.UtcNow),
                        "overdue" => query.Where(r =>
                            r.Milestones.SelectMany(m => m.Sections)
                                        .SelectMany(s => s.ToDoTasks)
                                        .Max(t => (DateTime?)t.DateEnd) < DateTime.UtcNow),
                        _ => query
                    };
                }

                if (!string.IsNullOrEmpty(request.Search))
                {
                    query = query.Where(r => r.Title.ToLower().Contains(request.Search.ToLower()));
                }

                string sortOrder = request.Asc == 1 ? "ascending" : "descending";
                string sortExpression = $"{request.SortBy} {sortOrder}";

                query = query.OrderBy(sortExpression);
               

                var totalCount = await query.CountAsync(cancellationToken);
                var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

                if (request.PageNumber > totalPages && totalPages > 0)
                {
                    throw new ValidationException(new List<FluentValidation.Results.ValidationFailure>
                    {
                        new("PageNumber", $"Page number {request.PageNumber} is out of range. Maximum page number is {totalPages}.")
                    });
                }

                var roadmaps = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync(cancellationToken);

                Log.Information("Get All Roadmap: {Roadmap}",
                JsonSerializer.Serialize(roadmaps, new JsonSerializerOptions
                {
                    ReferenceHandler = ReferenceHandler.Preserve
                }));

                return new PaginatedRoadmapResult<Roadmap>
                {
                    Items = roadmaps,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    CurrentPage = request.PageNumber,
                    PageSize = request.PageSize
                };
            }
        }
    }
}
