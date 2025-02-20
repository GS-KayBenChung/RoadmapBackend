//using Domain;
//using Domain.Dtos;
//using MediatR;
//using Persistence;
//public class CreateLogs
//{
//    public class Command : IRequest
//    {
//        public RoadmapLogsDto RoadmapLogsDto { get; set; }
//    }

//    public class Handler : IRequestHandler<Command>
//    {
//        private readonly DataContext _context;

//        public Handler(DataContext context)
//        {
//            _context = context;
//        }

//        public async Task Handle(Command request, CancellationToken cancellationToken)
//        {
//            var log = new AuditLog
//            {
//                LogId = Guid.NewGuid(),
//                UserId = request.RoadmapLogsDto.UserId,
//                ActivityAction = request.RoadmapLogsDto.ActivityAction,
//                CreatedAt = DateTime.UtcNow,
//            };

//            _context.AuditLogs.Add(log);
//            await _context.SaveChangesAsync(cancellationToken);
//        }
//    }

//}
using Domain;
using Domain.Dtos;
using FluentValidation;
using MediatR;
using Persistence;
using Serilog;
using Microsoft.EntityFrameworkCore;
using Application.Validator;

public class CreateLogs
{
    public class Command : IRequest
    {
        public RoadmapLogsDto RoadmapLogsDto { get; set; }
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
            await _validationService.ValidateAsync(request.RoadmapLogsDto, cancellationToken);

            var userExists = await _context.UserRoadmap.AnyAsync(u => u.UserId == request.RoadmapLogsDto.UserId, cancellationToken);
            if (!userExists)
            {
                throw new ValidationException(new List<FluentValidation.Results.ValidationFailure>
                {
                    new("UserId", "UserId does not exist in the database.")
                });
            }

            var log = new AuditLog
            {
                LogId = Guid.NewGuid(),
                UserId = request.RoadmapLogsDto.UserId,
                ActivityAction = request.RoadmapLogsDto.ActivityAction,
                CreatedAt = DateTime.UtcNow
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync(cancellationToken);

            Log.Information("Created log entry {LogId} for User {UserId}: {ActivityAction} at {Timestamp}",
                log.LogId, log.UserId, log.ActivityAction, log.CreatedAt);
        }

    }
}
