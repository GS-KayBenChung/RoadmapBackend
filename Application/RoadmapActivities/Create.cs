﻿using Domain;
using MediatR;
using Persistence;

namespace Application.RoadmapActivities
{
    public class Create
    {
        public class Command: IRequest
        {
            public Roadmap Roadmap { get; set; }

        }
        public class Handler : IRequestHandler<Command>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task Handle(Command request, CancellationToken cancellationToken)
            {
                _context.Roadmaps.Add(request.Roadmap);
                await _context.SaveChangesAsync();
            }

        }
    }
}
