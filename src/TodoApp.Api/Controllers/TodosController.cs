using Microsoft.AspNetCore.Mvc;
using TodoApp.Core;
using TodoApp.Api.DTOs;
using TodoApp.Api.Mapping;

namespace TodoApp.Api.Controllers;

[ApiController]
[Route("api/todos")]
[Produces("application/json")]
public class TodosController(ITodoService todoService) : ControllerBase
{
    /// <summary>
    /// Retrieves all To-Do items.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TodoResponse>>> GetAllTodos()
    {
        var todos = await todoService.GetAllAsync();
        var response = todos.Select(todo => todo.ToResponse());
        return Ok(response);        
    }

    /// <summary>
    /// Retrieves a specific To-Do item by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TodoResponse>> GetTodoById(Guid id)
    {
        var todo = await todoService.GetByIdAsync(id);
        return Ok(todo.ToResponse());
    }

    /// <summary>
    /// Creates a new To-Do item.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TodoResponse>> CreateTodo(CreateTodoRequest request)
    {
        var todo = await todoService.CreateAsync(request.Title);
        return CreatedAtAction(nameof(GetTodoById), new { id = todo.Id }, todo.ToResponse());
    }

    /// <summary>
    /// Updates the title of a specific To-Do item.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> UpdateTodo(Guid id, UpdateTodoRequest request)
    {
        await todoService.UpdateAsync(id, request.Title);
        return NoContent();
    }

    /// <summary>
    /// Deletes a specific To-Do item.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteTodo(Guid id)
    {
        await todoService.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Marks a specific To-Do item as completed.
    /// </summary>
    [HttpPost("{id}/complete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> CompleteTodo(Guid id)
    {
        await todoService.CompleteAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Reverts a completed To-Do item back to incomplete.
    /// </summary>
    [HttpPost("{id}/uncomplete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UncompleteTodo(Guid id)
    {
        await todoService.UncompleteAsync(id);
        return NoContent();
    }
}