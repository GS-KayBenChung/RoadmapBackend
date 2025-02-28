using Application.RoadmapActivities;
using Application.Validator;
using Domain;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Persistence;
using Assert = NUnit.Framework.Assert;

[TestFixture]
public class DeleteHandlerTests
{
    private DataContext? _context;
    private Mock<IValidationService>? _mockValidationService;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DataContext(options) ?? throw new InvalidOperationException("Failed to initialize DataContext.");
        _mockValidationService = new Mock<IValidationService>() ?? throw new InvalidOperationException("Failed to initialize ValidationService.");
    }

    [Test]
    public async Task Handle_WhenRoadmapExists_DeletesSuccessfully()
    {
        var roadmapId = Guid.NewGuid();

        var roadmap = new Roadmap
        {
            RoadmapId = roadmapId,
            Title = "Test Roadmap",
            IsDeleted = false,
            Milestones = new List<Milestone>
            {
                new Milestone
                {
                    Sections = new List<Section>
                    {
                        new Section
                        {
                            ToDoTasks = new List<ToDoTask>
                            {
                                new ToDoTask { IsDeleted = false }
                            },
                            IsDeleted = false
                        }
                    },
                    IsDeleted = false
                }
            }
        };

        _context.Roadmaps.Add(roadmap);
        await _context.SaveChangesAsync();

        var handler = new Delete.Handler(_context, _mockValidationService.Object);
        var command = new Delete.Command { Id = roadmapId };

        _mockValidationService
            .Setup(v => v.ValidateAsync(It.IsAny<Delete.Command>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await handler.Handle(command, CancellationToken.None);

        var deletedRoadmap = await _context.Roadmaps
            .Include(r => r.Milestones)
            .ThenInclude(m => m.Sections)
            .ThenInclude(s => s.ToDoTasks)
            .FirstOrDefaultAsync(r => r.RoadmapId == roadmapId);

        Assert.That(deletedRoadmap, Is.Not.Null, "Roadmap should exist in the database.");
        Assert.That(deletedRoadmap.IsDeleted, Is.True, "Roadmap should be marked as deleted.");
        Assert.That(deletedRoadmap.Milestones.All(m => m.IsDeleted), Is.True, "All milestones should be marked as deleted.");
        Assert.That(deletedRoadmap.Milestones.SelectMany(m => m.Sections).All(s => s.IsDeleted), Is.True, "All sections should be marked as deleted.");
        Assert.That(deletedRoadmap.Milestones.SelectMany(m => m.Sections).SelectMany(s => s.ToDoTasks).All(t => t.IsDeleted), Is.True, "All tasks should be marked as deleted.");
    }

    [Test]
    public void Handle_WhenRoadmapDoesNotExist_ThrowsValidationException()
    {
        var nonExistentRoadmapId = Guid.NewGuid();
        var handler = new Delete.Handler(_context, _mockValidationService.Object);
        var command = new Delete.Command { Id = nonExistentRoadmapId };

        _mockValidationService
            .Setup(v => v.ValidateAsync(It.IsAny<Delete.Command>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var ex = Assert.ThrowsAsync<ValidationException>(() => handler.Handle(command, CancellationToken.None));
        Assert.That(ex?.Errors.FirstOrDefault()?.ErrorMessage, Is.EqualTo("Roadmap not found."));
    }

    [Test]
    public async Task Handle_WhenRoadmapIsAlreadyDeleted_ThrowsValidationException()
    {
        var roadmapId = Guid.NewGuid();

        var roadmap = new Roadmap
        {
            RoadmapId = roadmapId,
            Title = "Already Deleted Roadmap",
            IsDeleted = true
        };

        _context.Roadmaps.Add(roadmap);
        await _context.SaveChangesAsync();

        var handler = new Delete.Handler(_context, _mockValidationService.Object);
        var command = new Delete.Command { Id = roadmapId };

        _mockValidationService
            .Setup(v => v.ValidateAsync(It.IsAny<Delete.Command>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var ex = Assert.ThrowsAsync<ValidationException>(() => handler.Handle(command, CancellationToken.None));
        Assert.That(ex?.Errors.FirstOrDefault()?.ErrorMessage, Is.EqualTo("Roadmap is already deleted."));
    }

    [TearDown]
    public void TearDown()
    {
        _context?.Dispose();
    }
}
