using System.ComponentModel.DataAnnotations;

namespace TodoApp.Api.DTOs;

public record UpdateTodoRequest
{
    [Required(ErrorMessage = "Todo title is required.")]
    [MaxLength(200, ErrorMessage = "Todo title cannot exceed 200 characters.")]
    public required string Title { get; init; }
}
