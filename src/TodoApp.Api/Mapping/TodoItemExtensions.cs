using TodoApp.Api.DTOs;
using TodoApp.Core;

namespace TodoApp.Api.Mapping;

public static class TodoItemExtensions
{
    public static TodoResponse ToResponse(this TodoItem item)
    {
        return new TodoResponse(
            item.Id,
            item.Title,
            item.IsCompleted,
            item.CreatedAt,
            item.CompletedAt
        );
    }
}
