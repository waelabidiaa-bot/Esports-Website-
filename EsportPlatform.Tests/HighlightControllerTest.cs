namespace EsportPlatform.Tests;

using EsportPlatform.API.Controllers;
using EsportPlatform.API.Data;
using EsportPlatform.API.Models;
using EsportPlatform.API.services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class HighlightControllerTest
{
    private (HighlightController controller, AppDbContext context) GetControllerWithContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);
        var service = new HighlightService(context);
        var controller = new HighlightController(service);

        return (controller, context);
    }

    private static async Task SeedGameAsync(AppDbContext context, int id = 1)
    {
        context.Games.Add(new Game { Id = id, Name = $"Game {id}", Genre = "FPS" });
        await context.SaveChangesAsync();
    }

    // ==========================================
    // GET TESTS
    // ==========================================
    [Fact]
    public async Task GetAllHighlights_ShouldReturnAllHighlights()
    {
        var (controller, context) = GetControllerWithContext();
        context.Highlights.AddRange(
            new Highlight { Id = 1, Title = "Grand Final Clutch", GameId = 1, VideoUrl = "https://example.com/1" },
            new Highlight { Id = 2, Title = "Top 10 Plays", GameId = 1, VideoUrl = "https://example.com/2" });
        await context.SaveChangesAsync();

        var result = await controller.GetAllHighlights();

        var ok = Assert.IsType<OkObjectResult>(result);
        var highlights = Assert.IsType<List<Highlight>>(ok.Value);
        Assert.Equal(2, highlights.Count);
    }

    [Fact]
    public async Task GetHighlightById_ShouldReturnHighlight_WhenItExists()
    {
        var (controller, context) = GetControllerWithContext();
        context.Highlights.Add(new Highlight { Id = 5, Title = "Top 10 Clutches", GameId = 1, VideoUrl = "https://example.com/5" });
        await context.SaveChangesAsync();

        var result = await controller.GetHighlightById(5);

        var ok = Assert.IsType<OkObjectResult>(result);
        var highlight = Assert.IsType<Highlight>(ok.Value);
        Assert.Equal("Top 10 Clutches", highlight.Title);
    }

    [Fact]
    public async Task GetHighlightById_ShouldReturnNotFound_WhenItDoesNotExist()
    {
        var (controller, _) = GetControllerWithContext();

        var result = await controller.GetHighlightById(999);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    // ==========================================
    // CREATE TESTS
    // ==========================================
    [Fact]
    public async Task CreateHighlight_ShouldReturnCreated_WhenGameExists()
    {
        var (controller, context) = GetControllerWithContext();
        await SeedGameAsync(context);

        var result = await controller.CreateHighlight(new Highlight
        {
            Title = "Opening Show",
            GameId = 1,
            VideoUrl = "https://www.youtube.com/watch?v=abc123",
            MatchLabel = "Team A vs Team B",
            Duration = "08:30",
            Views = 5000
        });

        var created = Assert.IsType<CreatedAtActionResult>(result);
        var highlight = Assert.IsType<Highlight>(created.Value);
        Assert.Equal("Opening Show", highlight.Title);
        Assert.Equal("https://www.youtube.com/watch?v=abc123", highlight.VideoUrl);
        Assert.NotEqual(default, highlight.PublishedAt);
    }

    [Fact]
    public async Task CreateHighlight_ShouldReturnBadRequest_WhenTitleIsMissing()
    {
        var (controller, _) = GetControllerWithContext();

        var result = await controller.CreateHighlight(new Highlight
        {
            Title = "  ",
            GameId = 1,
            VideoUrl = "https://example.com/video"
        });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreateHighlight_ShouldReturnBadRequest_WhenVideoUrlIsMissing()
    {
        var (controller, context) = GetControllerWithContext();
        await SeedGameAsync(context);

        var result = await controller.CreateHighlight(new Highlight
        {
            Title = "No video",
            GameId = 1,
            VideoUrl = "   "
        });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreateHighlight_ShouldReturnBadRequest_WhenGameDoesNotExist()
    {
        var (controller, _) = GetControllerWithContext();

        var result = await controller.CreateHighlight(new Highlight
        {
            Title = "Orphan Highlight",
            GameId = 42,
            VideoUrl = "https://example.com/video"
        });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    // ==========================================
    // UPDATE TESTS
    // ==========================================
    [Fact]
    public async Task UpdateHighlight_ShouldReturnOk_WhenValid()
    {
        var (controller, context) = GetControllerWithContext();
        await SeedGameAsync(context);
        context.Highlights.Add(new Highlight { Id = 1, Title = "Old Title", GameId = 1, Views = 100 });
        await context.SaveChangesAsync();

        var result = await controller.UpdateHighlight(new Highlight
        {
            Title = "New Title",
            GameId = 1,
            Views = 900,
            Duration = "10:00",
            MatchLabel = "Finals",
            VideoUrl = "https://www.youtube.com/watch?v=abc123"
        }, 1);

        var ok = Assert.IsType<OkObjectResult>(result);
        var highlight = Assert.IsType<Highlight>(ok.Value);
        Assert.Equal("New Title", highlight.Title);
        Assert.Equal(900, highlight.Views);
        Assert.Equal("https://www.youtube.com/watch?v=abc123", highlight.VideoUrl);
    }

    [Fact]
    public async Task UpdateHighlight_ShouldReturnNotFound_WhenItDoesNotExist()
    {
        var (controller, _) = GetControllerWithContext();

        var result = await controller.UpdateHighlight(new Highlight { Title = "Ghost", GameId = 1 }, 999);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task UpdateHighlight_ShouldReturnBadRequest_WhenGameDoesNotExist()
    {
        var (controller, context) = GetControllerWithContext();
        context.Highlights.Add(new Highlight { Id = 1, Title = "Existing", GameId = 1 });
        await context.SaveChangesAsync();

        var result = await controller.UpdateHighlight(new Highlight { Title = "Existing", GameId = 42 }, 1);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    // ==========================================
    // DELETE TESTS
    // ==========================================
    [Fact]
    public async Task DeleteHighlight_ShouldReturnNoContent_WhenItExists()
    {
        var (controller, context) = GetControllerWithContext();
        context.Highlights.Add(new Highlight { Id = 1, Title = "Delete Me", GameId = 1 });
        await context.SaveChangesAsync();

        var result = await controller.DeleteHighlight(1);

        Assert.IsType<NoContentResult>(result);
        Assert.Empty(context.Highlights);
    }

    [Fact]
    public async Task DeleteHighlight_ShouldReturnNotFound_WhenItDoesNotExist()
    {
        var (controller, _) = GetControllerWithContext();

        var result = await controller.DeleteHighlight(999);

        Assert.IsType<NotFoundObjectResult>(result);
    }
}
