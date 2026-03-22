using System.ComponentModel.DataAnnotations;

namespace TodoApp.Api.DTOs;

public record CreateTaskRequest
{
    [Required(ErrorMessage = "Task title is required.")]
    [MaxLength(200, ErrorMessage = "Task title cannot exceed 200 characters.")]
    public required string Title { get; init; }
}