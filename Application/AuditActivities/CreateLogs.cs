using System.Security.AccessControl;
using Domain;
using Domain.Dtos;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Serilog;

public class CreateLogs
{
    public class Command : IRequest
    {
        public RoadmapLogsDto RoadmapLogsDto { get; set; }
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

            Console.WriteLine($"UserId: {request.RoadmapLogsDto.UserId}, ActivityAction: {request.RoadmapLogsDto.ActivityAction}");

            var log = new AuditLog
            {
                LogId = Guid.NewGuid(),
                UserId = request.RoadmapLogsDto.UserId,
                ActivityAction = request.RoadmapLogsDto.ActivityAction,
                CreatedAt = DateTime.UtcNow,
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
