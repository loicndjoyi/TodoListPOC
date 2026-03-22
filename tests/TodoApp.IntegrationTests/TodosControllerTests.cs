using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TodoApp.Api.DTOs;
using TodoApp.Infrastructure;
using FluentAssertions;

namespace TodoApp.IntegrationTests;

/// <summary>
/// Custom factory that replaces the SQLite file DB with a fresh in-memory connection per test.
/// Each test class instantiates its own factory to guarantee complete isolation.
/// </summary>
public class TodoAppFactory : WebApplicationFactory<Program>, IDisposable
{
    private readonly SqliteConnection _connection;

    public TodoAppFactory()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the real SQLite DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            // Register with the in-memory connection (shared so EnsureCreated works)
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(_connection));
        });
    }

    public HttpClient CreateIsolatedClient()
    {
        var client = CreateClient();

        // Ensure the schema is created fresh for this test instance
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();

        return client;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Dispose();
        }
    }
}

public class TodosControllerTests : IDisposable
{
    private readonly TodoAppFactory _factory;
    private readonly HttpClient _client;

    public TodosControllerTests()
    {
        _factory = new TodoAppFactory();
        _client = _factory.CreateIsolatedClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task GetAllTodos_EmptyDatabase_Returns200WithEmptyList()
    {
        // Act
        var response = await _client.GetAsync("/api/todos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var todos = await response.Content.ReadFromJsonAsync<IEnumerable<TodoResponse>>();
        todos.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateTodo_ValidInput_Returns201WithLocationHeader()
    {
        // Arrange
        var request = new CreateTodoRequest { Title = "Integration Test Task" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/todos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var todo = await response.Content.ReadFromJsonAsync<TodoResponse>();
        todo.Should().NotBeNull();
        todo!.Title.Should().Be("Integration Test Task");
    }

    [Fact]
    public async Task CreateTodo_EmptyTitle_Returns400BadRequest()
    {
        // Arrange
        var request = new CreateTodoRequest { Title = "" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/todos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetTodoById_ExistingId_Returns200WithTodo()
    {
        // Arrange
        var createRequest = new CreateTodoRequest { Title = "Task to find" };
        var createResponse = await _client.PostAsJsonAsync("/api/todos", createRequest);
        var createdTodo = await createResponse.Content.ReadFromJsonAsync<TodoResponse>();

        // Act
        var response = await _client.GetAsync($"/api/todos/{createdTodo!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var todo = await response.Content.ReadFromJsonAsync<TodoResponse>();
        todo!.Id.Should().Be(createdTodo.Id);
    }

    [Fact]
    public async Task GetTodoById_NonExistingId_Returns404NotFound()
    {
        // Act
        var response = await _client.GetAsync($"/api/todos/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateTodo_ExistingId_Returns204AndUpdatesTitle()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync("/api/todos", new CreateTodoRequest { Title = "Old Title" });
        var createdTodo = await createResponse.Content.ReadFromJsonAsync<TodoResponse>();

        // Act
        var response = await _client.PutAsJsonAsync($"/api/todos/{createdTodo!.Id}", new UpdateTodoRequest { Title = "New Title" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetFromJsonAsync<TodoResponse>($"/api/todos/{createdTodo.Id}");
        getResponse!.Title.Should().Be("New Title");
    }

    [Fact]
    public async Task DeleteTodo_ExistingId_Returns204AndRemoves()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync("/api/todos", new CreateTodoRequest { Title = "Task to Delete" });
        var createdTodo = await createResponse.Content.ReadFromJsonAsync<TodoResponse>();

        // Act
        var response = await _client.DeleteAsync($"/api/todos/{createdTodo!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var checkResponse = await _client.GetAsync($"/api/todos/{createdTodo.Id}");
        checkResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CompleteTodo_ExistingId_Returns204AndMarksCompleted()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync("/api/todos", new CreateTodoRequest { Title = "Task to Complete" });
        var createdTodo = await createResponse.Content.ReadFromJsonAsync<TodoResponse>();

        // Act
        var response = await _client.PostAsync($"/api/todos/{createdTodo!.Id}/complete", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetFromJsonAsync<TodoResponse>($"/api/todos/{createdTodo.Id}");
        getResponse!.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task UncompleteTodo_CompletedTodo_Returns204AndResetsState()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync("/api/todos", new CreateTodoRequest { Title = "Task to Uncomplete" });
        var createdTodo = await createResponse.Content.ReadFromJsonAsync<TodoResponse>();
        await _client.PostAsync($"/api/todos/{createdTodo!.Id}/complete", null);

        // Act
        var response = await _client.PostAsync($"/api/todos/{createdTodo.Id}/uncomplete", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetFromJsonAsync<TodoResponse>($"/api/todos/{createdTodo.Id}");
        getResponse!.IsCompleted.Should().BeFalse();
    }
}
