using System.ComponentModel.DataAnnotations;

namespace TodoApp.Api.DTOs;

/// <summary>
/// Data transfer object for updating an existing To-Do item.
/// </summary>
public record UpdateTodoRequest
{
    /// <summary>
    /// The new title for the To-Do item.
    /// </summary>
    /// <example>Buy organic groceries</example>
    [Required(ErrorMessage = "Todo title is required.")]
    [MaxLength(200, ErrorMessage = "Todo title cannot exceed 200 characters.")]
    public required string Title { get; init; }
}
