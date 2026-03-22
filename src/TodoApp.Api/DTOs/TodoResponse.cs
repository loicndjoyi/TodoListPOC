namespace TodoApp.Api.DTOs;

public record TodoResponse(Guid Id, string Title, bool IsCompleted, DateTime CreatedAt, DateTime? CompletedAt);