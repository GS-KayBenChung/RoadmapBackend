using AutoMapper;
using Domain;
using MediatR;
using Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.RoadmapActivities
{
    public class Delete
    {
        public class Command : IRequest
        {
            public Guid Id { get; set; }

        }
        public class Handler : IRequestHandler<Command>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            //public async Task Handle(Command request, CancellationToken cancellationToken)
            //{
            //    var roadmap = await _context.Roadmaps.FindAsync(request.Id);

            //    _context.Remove(roadmap);

            //    await _context.SaveChangesAsync();
            //}

            public async Task Handle(Command request, CancellationToken cancellationToken)
            {
                var roadmap = await _context.Roadmaps.FindAsync(request.Id);
                if (roadmap == null)
                {
                    return ;
                }

                roadmap.IsDeleted = true;
                _context.Roadmaps.Update(roadmap);
                await _context.SaveChangesAsync();
            }
        }
    }
}
