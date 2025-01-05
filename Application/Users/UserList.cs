using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Roadmaps
{
    public class UserList
    {
        public class Query : IRequest<List<UserRoadmap>> { }

        public class Handler : IRequestHandler<Query, List<UserRoadmap>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<List<UserRoadmap>> Handle(Query request, CancellationToken cancellationToken)
            {
                return await _context.UserRoadmap.ToListAsync();
            }
        }
    }
}