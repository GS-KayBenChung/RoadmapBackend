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
public class GetDetailsHandlerTests
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
    public async Task Handle_WhenRoadmapExists_ReturnsRoadmapDetails()
    {
        var roadmapId = Guid.NewGuid();
        var testUserId = Guid.NewGuid();

        var roadmap = new Roadmap
        {
            RoadmapId = roadmapId,
            Title = "Test Roadmap",
            Description = "A sample roadmap",
            CreatedBy = testUserId,
            OverallProgress = 50,
            OverallDuration = 30,
            IsCompleted = false,
            IsDraft = false,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Milestones = new List<Milestone>
            {
                new Milestone
                {
                    MilestoneId = Guid.NewGuid(),
                    RoadmapId = roadmapId,
                    Name = "Milestone 1",
                    Description = "First milestone",
                    IsCompleted = false,
                    Sections = new List<Section>
                    {
                        new Section
                        {
                            SectionId = Guid.NewGuid(),
                            Name = "Section 1",
                            Description = "First section",
                            IsCompleted = false,
                            ToDoTasks = new List<ToDoTask>
                            {
                                new ToDoTask
                                {
                                    TaskId = Guid.NewGuid(),
                                    Name = "Task 1",
                                    DateStart = DateTime.UtcNow,
                                    DateEnd = DateTime.UtcNow.AddDays(3),
                                    IsCompleted = false
                                }
                            }
                        }
                    }
                }
            }
        };

        _context.Roadmaps.Add(roadmap);
        await _context.SaveChangesAsync();

        var handler = new GetDetails.Handler(_context, _mockValidationService.Object);
        var query = new GetDetails.Query { Id = roadmapId };

        _mockValidationService
            .Setup(v => v.ValidateAsync(It.IsAny<GetDetails.Query>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.That(result, Is.Not.Null, "Roadmap should be retrieved successfully.");
        Assert.That(result.RoadmapId, Is.EqualTo(roadmapId), "Roadmap ID should match.");
        Assert.That(result.Title, Is.EqualTo("Test Roadmap"), "Title should match.");
        Assert.That(result.Milestones.Count, Is.EqualTo(1), "Milestones count should be correct.");
        Assert.That(result.Milestones[0].Sections.Count, Is.EqualTo(1), "Sections count should be correct.");
        Assert.That(result.Milestones[0].Sections[0].Tasks.Count, Is.EqualTo(1), "Tasks count should be correct.");
    }

    [Test]
    public void Handle_WhenRoadmapDoesNotExist_ThrowsValidationException()
    {
        var nonExistentRoadmapId = Guid.NewGuid();
        var handler = new GetDetails.Handler(_context, _mockValidationService.Object);
        var query = new GetDetails.Query { Id = nonExistentRoadmapId };

        _mockValidationService
            .Setup(v => v.ValidateAsync(It.IsAny<GetDetails.Query>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var ex = Assert.ThrowsAsync<ValidationException>(() => handler.Handle(query, CancellationToken.None));
        Assert.That(ex?.Errors.FirstOrDefault()?.ErrorMessage, Is.EqualTo("Roadmap not found."));
    }

    [Test]
    public async Task Handle_WhenRoadmapIsDeleted_ThrowsValidationException()
    {
        var roadmapId = Guid.NewGuid();

        var roadmap = new Roadmap
        {
            RoadmapId = roadmapId,
            Title = "Deleted Roadmap",
            IsDeleted = true
        };

        _context.Roadmaps.Add(roadmap);
        await _context.SaveChangesAsync();

        var handler = new GetDetails.Handler(_context, _mockValidationService.Object);
        var query = new GetDetails.Query { Id = roadmapId };

        _mockValidationService
            .Setup(v => v.ValidateAsync(It.IsAny<GetDetails.Query>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var ex = Assert.ThrowsAsync<ValidationException>(() => handler.Handle(query, CancellationToken.None));
        Assert.That(ex?.Errors.FirstOrDefault()?.ErrorMessage, Is.EqualTo("This roadmap has been deleted and cannot be retrieved."));
    }

    [TearDown]
    public void TearDown()
    {
        _context?.Dispose();
    }
}
