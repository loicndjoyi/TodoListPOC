using System.ComponentModel.DataAnnotations;

namespace TodoApp.Api.DTOs;

/// <summary>
/// Data transfer object for creating a new To-Do item.
/// </summary>
public record CreateTodoRequest
{
    /// <summary>
    /// The title of the To-Do item.
    /// </summary>
    /// <example>Buy groceries</example>
    [Required(ErrorMessage = "Todo title is required.")]
    [MaxLength(200, ErrorMessage = "Todo title cannot exceed 200 characters.")]
    public required string Title { get; init; }
}