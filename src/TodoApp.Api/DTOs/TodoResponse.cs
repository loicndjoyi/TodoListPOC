namespace TodoApp.Api.DTOs;

/// <summary>
/// Represents a To-Do item returned by the API.
/// </summary>
/// <param name="Id">The unique identifier of the To-Do item.</param>
/// <param name="Title">The title or description of the task.</param>
/// <param name="IsCompleted">Indicates whether the task has been completed.</param>
/// <param name="CreatedAt">The UTC timestamp when the task was created.</param>
/// <param name="CompletedAt">The UTC timestamp when the task was completed, or null if incomplete.</param>
public record TodoResponse(Guid Id, string Title, bool IsCompleted, DateTime CreatedAt, DateTime? CompletedAt);