# Technical Learnings & Decisions

This document captures key technical learnings, architectural decisions, and design patterns discovered or explicitly defined during the project.

---

## 1. The `init` Accessor for Immutability

The `init` accessor (introduced in C# 9.0) ensures a property can only be set during object instantiation (in the constructor or via an object initializer). After initialization, it becomes strictly read-only.

- **Why use it?**: It protects fundamental identity fields (like `Id` and `CreatedAt`) from accidental reassignment during the entity's lifecycle.
- **ORM Benefit**: It allows libraries like EF Core or `System.Text.Json` to cleanly populate these fields during reconstruction from the database or network, without compromising long-term immutability.

```csharp
public class TodoItem
{
    // Can only be set during object creation.
    public Guid Id { get; init; }
    public DateTime CreatedAt { get; init; }
}

// var target = new TodoItem();
// target.Id = Guid.NewGuid(); // COMPILATION ERROR
```

## 2. Nullable Reference Types (NRT) & EF Core `DbSet`

**Date**: 2026-03-22

When Nullable Reference Types (`<Nullable>enable</Nullable>`) are turned on, C# compiler warning CS8618 complains if non-nullable properties like `DbSet<TodoItem>` are not initialized in the constructor. However, Entity Framework Core dynamically initializes DbSets via reflection under the hood.

To cleanly bypass the warning in modern .NET without using the null-forgiving operator (`= null!`), there are two approaches:

1. **The `required` keyword (C# 11 / .NET 7+)**:
   Uses `required` to instruct the compiler that this property must be set at construction. EF Core satisfies this via reflection. However, **this causes complications when instantiating the DbContext manually in tests** (CS9035 error), since `required` demands the property be set in the object initializer.

   ```csharp
   public required DbSet<TodoItem> TodoItems { get; set; }
   ```

2. **The `=> Set<T>()` Expression-bodied Property (our choice) ✅**:
   Dynamically calls EF's generic `Set<T>()` method each time the property is accessed. No backing field, avoids the CS8618 warning, and **works seamlessly in test contexts** where we instantiate `AppDbContext` with custom `DbContextOptions`.

   ```csharp
   public DbSet<TodoItem> TodoItems => Set<TodoItem>();
   ```

## 3. Primary Constructors (C# 12)

Primary constructors in C# 12 heavily reduce Dependency Injection boilerplate. By declaring dependencies at the class definition level, we eliminate the need to define private readonly fields and the constructor body explicitly.

```csharp
// Before C# 12
public class TodoService : ITodoService
{
    private readonly AppDbContext _context;
    public TodoService(AppDbContext context)
    {
        _context = context;
    }
}

// With Primary Constructors (C# 12)
public class TodoService(AppDbContext context) : ITodoService
{
    // 'context' is directly accessible throughout the class methods!
}
```

## 4. Clean Code: Guard Clauses & `is null`

Using early returns (Guard Clauses) avoids deep nesting ("Arrow Code"). Furthermore, using C# pattern matching `is null` (or `is not null`) guarantees a true null reference check, circumventing any potential overloaded `==` equality operators on the object.

```csharp
public async Task CompleteAsync(Guid id)
{
    var todoItem = await context.TodoItems.FindAsync(id);
    if (todoItem is null) return; // Guard clause + true reference check

    todoItem.Complete();
    await context.SaveChangesAsync();
}
```

## 5. Global Exception Handling (RFC 7807) in .NET 8

Instead of leaking stack traces or returning generic 500 HTML pages when an error occurs, modern .NET 8 APIs should use `IExceptionHandler` combined with `ProblemDetails` (`AddProblemDetails()`). This maps backend exceptions (e.g. `KeyNotFoundException`) into a standardized RFC 7807 JSON schema for safe and predictable API consumption.

```csharp
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problemDetails = new ProblemDetails
        {
            Status = exception is KeyNotFoundException ? StatusCodes.Status404NotFound : StatusCodes.Status500InternalServerError,
            Title = exception is KeyNotFoundException ? "Resource Not Found" : "Server Error",
            Detail = exception.Message
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}
```

## 6. Resolving `dotnet-ef` Global Tool Issues on macOS Homebrew

When using Homebrew on Apple Silicon (arm64), global `dotnet-ef` installations can sometimes fail to launch with `.NET location: Not found` because Homebrew isolates its runtime paths differently than official installers.
**Solution**: Use a local tool manifest! This ensures `dotnet-ef` maps perfectly to the workspace's specific .NET SDK version and avoids architecture confusion. Also ensure your API Startup Project has the `Microsoft.EntityFrameworkCore.Design` dependency.

```bash
dotnet new tool-manifest
dotnet tool install dotnet-ef --version 8.0.25
dotnet tool run dotnet-ef migrations add InitialCreate --project src/TodoApp.Infrastructure --startup-project src/TodoApp.Api
```

## 7. Testing Best Practices (.NET 8 — xUnit + FluentAssertions)

### 7a. No `Thread.Sleep` in Tests
Tests must never rely on real wall-clock time (flaky, slow, non-deterministic). If the goal is to verify that a value was **not changed**, assert the value directly — a `Thread.Sleep` is a code smell that signals the test is doing too much.

```csharp
// ❌ Bad — fragile and slow
Thread.Sleep(5);
todo.Complete();
todo.CompletedAt.Should().Be(initialCompletedAt);

// ✅ Good — assert the value was preserved without any delay
todo.Complete();
todo.CompletedAt.Should().Be(initialCompletedAt);
```

### 7b. EF Core Change Tracker — The Cache Trap
When testing a service method that updates a record and then re-reading the record via `FindAsync` on the **same `DbContext` instance**, EF Core can return the **in-memory cached value** rather than going back to the database. This creates a false positive: the test passes even if `SaveChangesAsync` was never called.

**Fix**: Call `_context.ChangeTracker.Clear()` before re-reading the entity.

```csharp
// ❌ Dangerous — EF may serve this from its in-memory cache
await _service.UpdateAsync(item.Id, "New Title");
var dbItem = await _context.TodoItems.FindAsync(item.Id);
dbItem!.Title.Should().Be("New Title"); // could pass even if DB was NOT updated

// ✅ Safe — forces a real DB round-trip
await _service.UpdateAsync(item.Id, "New Title");
_context.ChangeTracker.Clear();
var dbItem = await _context.TodoItems.FindAsync(item.Id);
dbItem!.Title.Should().Be("New Title");
```

### 7c. Integration Test Isolation with `WebApplicationFactory`
Using `IClassFixture<WebApplicationFactory<Program>>` shares the same factory instance across all tests in a class. If tests write data to the DB (our in-memory SQLite), that data **bleeds into other tests**, making them order-dependent.

**Fix**: Create a custom `WebApplicationFactory` subclass and instantiate it fresh **per test** via `IDisposable`, giving each test its own clean SQLite in-memory connection.

```csharp
// ✅ Custom factory — gives each test a fresh database
public class TodoAppFactory : WebApplicationFactory<Program>, IDisposable
{
    private readonly SqliteConnection _connection;

    public TodoAppFactory()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the real DB registration and replace with in-memory SQLite
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null) services.Remove(descriptor);
            services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));
        });
    }
}

// Each test class instantiates its own factory — fully isolated
public class TodosControllerTests : IDisposable
{
    private readonly TodoAppFactory _factory;
    public TodosControllerTests() { _factory = new TodoAppFactory(); }
    public void Dispose() { _factory.Dispose(); }
}
```

### 7f. The Dispose Pattern & `GC.SuppressFinalize`
When a class implements `IDisposable`, it is best practice to call `GC.SuppressFinalize(this)` at the end of the `Dispose()` method. This tells the Garbage Collector that the object has already been cleaned up and it doesn't need to call the finalizer (if one exists), which improves performance.

```csharp
public void Dispose()
{
    // Clean up resources
    _client.Dispose();
    _factory.Dispose();
    
    // ✅ Tell GC not to call the finalizer
    GC.SuppressFinalize(this);
}
```

### 7d. Test Placement: Unit vs. Integration
Tests that use a **real database** (even SQLite in-memory) are **integration tests**, not unit tests. A true unit test has zero I/O — no DB, no HTTP, no file system.

| What | Project | Why |
|---|---|---|
| `TodoItem` entity (domain rules) | `UnitTests` | Pure logic, no dependencies |
| `TodoService` (uses `AppDbContext`) | `IntegrationTests` | Requires a real DB connection |
| `TodosController` (HTTP pipeline) | `IntegrationTests` | Requires `WebApplicationFactory` |

### 7e. Test Naming Convention
Follow the pattern `MethodName_Scenario_ExpectedResult` for consistency and readability.

```csharp
// ❌ Bad — verbose and inconsistent
public void UpdateTitle_ShouldThrowArgumentException_WhenTitleIsNullOrWhitespace()

// ✅ Good — concise and follows the convention
public void UpdateTitle_NullOrWhitespaceTitle_ThrowsArgumentException()
```

---

## 8. Angular 21: Signals & `ChangeDetectionStrategy.OnPush`

Angular Signals (`signal()`, `computed()`) replace the need for RxJS `BehaviorSubject` for local component state. When combined with `ChangeDetectionStrategy.OnPush`, Angular only re-renders a component when a Signal value changes or an `@Input` reference changes — not on every browser event.

```typescript
// State
readonly todos = signal<Todo[]>([]);
readonly isLoading = signal(false);

// Derived (recomputed automatically when todos() changes)
readonly completedCount = computed(() => this.todos().filter(t => t.isCompleted).length);
```

> **Key rule**: Always use `update()` or `set()` on signals — never `mutate()`.

## 9. Angular 21: `input()` / `output()` Functions vs Decorators

Since Angular v16+, the `input()` and `output()` functions replace `@Input()` and `@Output()` decorators. They provide better type inference and are the recommended pattern in GEMINI.md.

```typescript
// ❌ Old decorator style
@Input() initialTitle = '';
@Output() save = new EventEmitter<string>();

// ✅ Modern function style
readonly initialTitle = input('');
readonly save = output<string>();
```

## 10. Angular v20+: `standalone: true` Is the Default

Starting with Angular v20, all components, directives, and pipes are **standalone by default**. Setting `standalone: true` explicitly in the `@Component` decorator is redundant and should be removed. Similarly, `CommonModule` is no longer needed when using native control flow (`@if`, `@for`, `@switch`).

```typescript
// ❌ Redundant in v20+
@Component({
  standalone: true,
  imports: [CommonModule],
})

// ✅ Clean
@Component({
  imports: [ReactiveFormsModule],
})
```

## 11. ASP.NET Core: CORS Middleware Ordering

In the ASP.NET Core middleware pipeline, `UseCors()` must be placed **before** `UseHttpsRedirection()`. If HTTPS redirection runs first, it issues a 307 redirect before the CORS headers are attached, causing the browser to reject the preflight response.

```csharp
// ❌ Broken — redirect strips CORS headers
app.UseHttpsRedirection();
app.UseCors("AllowAngularClient");

// ✅ Correct — CORS headers are set before any redirect
app.UseCors("AllowAngularClient");
app.UseHttpsRedirection();
```

## 12. Angular 21: Runtime Environment Detection without `environments/`

Modern Angular CLI workspaces (v15+) do not generate an `environments/` directory by default to keep the workspace simple. Rather than manually configuring proxy files or restoring the environment infrastructure for a simple endpoint swap, the cleanest native approach is using `@angular/core`'s `isDevMode()`.

```typescript
import { isDevMode } from '@angular/core';

export class TodoService {
  // Uses absolute path for ng serve, and relative path automatically after 'ng build'
  private baseUrl = isDevMode() ? 'http://localhost:5118/api/todos' : '/api/todos';
}
```
