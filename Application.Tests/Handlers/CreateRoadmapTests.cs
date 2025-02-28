using Domain.Dtos;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Persistence;
using Moq;
using Application.Validator;
using Assert = NUnit.Framework.Assert;
using Domain;

[TestFixture]
public class CreateHandlerTests
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
    public async Task Handle_ValidData_CreatesRoadmap()
    {
        var testUserId = new Guid("0e7d3f8c-845c-4c69-b50d-9f07c0c7b98f");

        var testUser = new UserRoadmap
        {
            UserId = testUserId, 
            Name = "Test User",
            Email = "test@example.com",
            GoogleId = "123456",
            CreatedAt = DateTime.UtcNow
        };
        _context?.UserRoadmap.Add(testUser);
        await _context.SaveChangesAsync();

        var handler = new Create.Handler(_context, _mockValidationService?.Object);

        var roadmapDto = new CreateRoadmapDto
        {
            Title = "Test Roadmap",
            Description = "A sample roadmap",
            CreatedBy = testUserId,
            IsDraft = false,
            OverallDuration = 10
        };

        var command = new Create.Command { RoadmapDto = roadmapDto };

        _mockValidationService?
            .Setup(v => v.ValidateAsync(It.IsAny<CreateRoadmapDto>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await handler.Handle(command, CancellationToken.None);

        var roadmap = await _context.Roadmaps.FirstOrDefaultAsync(r => r.Title == roadmapDto.Title);

        Assert.That(roadmap, Is.Not.Null, "Roadmap should be created.");
        Assert.That(roadmap?.Title, Is.EqualTo(roadmapDto.Title), "Title should match.");
        Assert.That(roadmap?.CreatedBy, Is.EqualTo(roadmapDto.CreatedBy), "User ID should match.");
    }



    [TearDown] 
    public void TearDown()
    {
        _context.Dispose();
    }
}
