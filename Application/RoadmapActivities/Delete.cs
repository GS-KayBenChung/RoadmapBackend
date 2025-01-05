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
            public async Task Handle(Command request, CancellationToken cancellationToken)
            {
                try
                {
                    var roadmap = await _context.Roadmaps.FindAsync(request.Id);

                    if (roadmap == null)
                    {
                        throw new KeyNotFoundException("Roadmap not found.");
                    }
           
                    roadmap.IsDeleted = true;
            
                    _context.Roadmaps.Update(roadmap);

                    await _context.SaveChangesAsync(cancellationToken);
                }
                catch (KeyNotFoundException ex)
                {
                    throw new ApplicationException($"Error: {ex.Message}");
                }
                catch (Exception ex)
                {
                    throw new ApplicationException("An unexpected error occurred while deleting the roadmap." + ex);
                }
            }
        }
    }
}