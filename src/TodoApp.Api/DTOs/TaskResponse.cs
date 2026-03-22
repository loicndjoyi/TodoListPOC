namespace TodoApp.Api.DTOs;

public record TaskResponse(Guid Id, string Title, bool IsCompleted, DateTime CreatedAt, DateTime? CompletedAt);