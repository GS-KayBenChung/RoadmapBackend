using AutoMapper;
using Domain;
using MediatR;
using Persistence;

namespace Application.RoadmapActivities
{
    public class Edit
    {
        public class Command : IRequest
        {
            public Roadmap Roadmap { get; set; }

        }
        public class Handler : IRequestHandler<Command>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task Handle(Command request, CancellationToken cancellationToken)
            {
                var roadmap = await _context.Roadmaps.FindAsync(request.Roadmap.RoadmapId);

                _mapper.Map(request.Roadmap, roadmap);

                await _context.SaveChangesAsync();
            }

        }
    }
}
