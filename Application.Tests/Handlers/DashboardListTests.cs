using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.RoadmapActivities;
using Domain;
using Domain.Dtos;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Persistence;
using Assert = NUnit.Framework.Assert;

[TestFixture] 
public class DashboardListHandlerTests
{
    private DataContext? _context;

    [SetUp] 
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) 
            .Options;

        _context = new DataContext(options);
    }

    [Test] 
    public async Task Handle_WhenRoadmapsExist_ReturnsCorrectStatistics()
    {

        var roadmap1 = new Roadmap
        {
            RoadmapId = Guid.NewGuid(),
            Title = "Completed Roadmap",
            IsCompleted = true,
            IsDraft = false,
            CreatedBy = Guid.NewGuid(),
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
                                new ToDoTask { DateEnd = DateTime.UtcNow.AddDays(3), IsDeleted = false }
                            }
                        }
                    }
                }
            }
        };

        var roadmap2 = new Roadmap
        {
            RoadmapId = Guid.NewGuid(),
            Title = "Draft Roadmap",
            IsCompleted = false,
            IsDraft = true,
            CreatedBy = Guid.NewGuid(),
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
                                new ToDoTask { DateEnd = DateTime.UtcNow.AddDays(-2), IsDeleted = false }
                            }
                        }
                    }
                }
            }
        };

        var roadmap3 = new Roadmap
        {
            RoadmapId = Guid.NewGuid(),
            Title = "Overdue Roadmap",
            IsCompleted = false,
            IsDraft = false,
            CreatedBy = Guid.NewGuid(),
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
                                new ToDoTask { DateEnd = DateTime.UtcNow.AddDays(-1), IsDeleted = false }
                            }
                        }
                    }
                }
            }
        };

        _context.Roadmaps.AddRange(roadmap1, roadmap2, roadmap3);
        await _context.SaveChangesAsync();

        var handler = new DashboardList.Handler(_context);
        var query = new DashboardList.Query();

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.That(result.TotalRoadmaps, Is.EqualTo(3), "Total roadmaps count should be correct.");
        Assert.That(result.CompletedRoadmaps, Is.EqualTo(1), "Completed roadmaps count should be correct.");
        Assert.That(result.DraftRoadmaps, Is.EqualTo(1), "Draft roadmaps count should be correct.");
        Assert.That(result.PublishedRoadmaps, Is.EqualTo(2), "Published roadmaps count should be correct.");
        Assert.That(result.NearDueRoadmaps, Is.EqualTo(1), "Near-due roadmaps count should be correct.");
        Assert.That(result.OverdueRoadmaps, Is.EqualTo(1), "Overdue roadmaps count should be correct.");
    }

    [Test] 
    public async Task Handle_WhenNoRoadmapsExist_ReturnsZeroStatistics()
    {
        var handler = new DashboardList.Handler(_context);
        var query = new DashboardList.Query();

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.That(result.TotalRoadmaps, Is.EqualTo(0), "Total roadmaps should be 0.");
        Assert.That(result.CompletedRoadmaps, Is.EqualTo(0), "Completed roadmaps should be 0.");
        Assert.That(result.DraftRoadmaps, Is.EqualTo(0), "Draft roadmaps should be 0.");
        Assert.That(result.PublishedRoadmaps, Is.EqualTo(0), "Published roadmaps should be 0.");
        Assert.That(result.NearDueRoadmaps, Is.EqualTo(0), "Near-due roadmaps should be 0.");
        Assert.That(result.OverdueRoadmaps, Is.EqualTo(0), "Overdue roadmaps should be 0.");
    }

    [TearDown] 
    public void TearDown()
    {
        _context.Dispose();
    }
}
