using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TodoApp.Core;
using TodoApp.Infrastructure;
using TodoApp.Infrastructure.Services;

namespace TodoApp.IntegrationTests;

public class TodoServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _context;
    private readonly TodoService _service;

    public TodoServiceTests()
    {
        // Use a persistent in-memory SQLite connection for each test
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();

        _service = new TodoService(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task CreateAsync_ValidTitle_AddsTodoAndReturnsIt()
    {
        // Arrange
        var title = "Test Action";

        // Act
        var result = await _service.CreateAsync(title);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be(title);
        result.Id.Should().NotBeEmpty();

        _context.ChangeTracker.Clear();
        var dbItem = await _context.TodoItems.FindAsync(result.Id);
        dbItem.Should().NotBeNull();
        dbItem!.Title.Should().Be(title);
    }

    [Fact]
    public async Task GetAllAsync_TwoItemsSeeded_ReturnsBoth()
    {
        // Arrange
        await _service.CreateAsync("Task 1");
        await _service.CreateAsync("Task 2");

        // Act
        var results = await _service.GetAllAsync();

        // Assert
        results.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsCorrectItem()
    {
        // Arrange
        var item = await _service.CreateAsync("Task Find Me");

        // Act
        var result = await _service.GetByIdAsync(item.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(item.Id);
        result.Title.Should().Be("Task Find Me");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ThrowsKeyNotFoundException()
    {
        // Act
        Func<Task> act = async () => await _service.GetByIdAsync(Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
                 .WithMessage("*not found*");
    }

    [Fact]
    public async Task UpdateAsync_ExistingId_ChangesTitleInDatabase()
    {
        // Arrange
        var item = await _service.CreateAsync("Old Title");

        // Act
        await _service.UpdateAsync(item.Id, "New Title");

        // Assert — detach to bypass EF change tracker cache and truly read from DB
        _context.ChangeTracker.Clear();
        var dbItem = await _context.TodoItems.FindAsync(item.Id);
        dbItem!.Title.Should().Be("New Title");
    }

    [Fact]
    public async Task CompleteAsync_IncompleteTodo_MarksAsCompletedInDatabase()
    {
        // Arrange
        var item = await _service.CreateAsync("Task to Complete");

        // Act
        await _service.CompleteAsync(item.Id);

        // Assert — detach to bypass EF change tracker cache and truly read from DB
        _context.ChangeTracker.Clear();
        var dbItem = await _context.TodoItems.FindAsync(item.Id);
        dbItem!.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task UncompleteAsync_CompletedTodo_ResetsCompletionInDatabase()
    {
        // Arrange
        var item = await _service.CreateAsync("Task to Uncomplete");
        await _service.CompleteAsync(item.Id);

        // Act
        await _service.UncompleteAsync(item.Id);

        // Assert — detach to bypass EF change tracker cache and truly read from DB
        _context.ChangeTracker.Clear();
        var dbItem = await _context.TodoItems.FindAsync(item.Id);
        dbItem!.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ExistingId_RemovesItemFromDatabase()
    {
        // Arrange
        var item = await _service.CreateAsync("Task to Delete");

        // Act
        await _service.DeleteAsync(item.Id);

        // Assert
        var dbItem = await _context.TodoItems.FindAsync(item.Id);
        dbItem.Should().BeNull();
    }

    [Fact]
    public async Task GetCompletedAsync_MixedItems_ReturnsOnlyCompleted()
    {
        // Arrange
        var completedItem = await _service.CreateAsync("Task 1");
        await _service.CreateAsync("Task 2"); // incomplete - not referenced intentionally
        await _service.CompleteAsync(completedItem.Id);

        // Act
        var results = await _service.GetCompletedAsync();

        // Assert
        results.Should().ContainSingle();
        results.First().Id.Should().Be(completedItem.Id);
    }

    [Fact]
    public async Task GetIncompleteAsync_MixedItems_ReturnsOnlyIncomplete()
    {
        // Arrange
        var completedItem = await _service.CreateAsync("Task 1");
        var incompleteItem = await _service.CreateAsync("Task 2");
        await _service.CompleteAsync(completedItem.Id);

        // Act
        var results = await _service.GetIncompleteAsync();

        // Assert
        results.Should().ContainSingle();
        results.First().Id.Should().Be(incompleteItem.Id);
    }
}
