using Microsoft.AspNetCore.Mvc;
using TodoApp.Core;
using TodoApp.Api.DTOs;
using TodoApp.Api.Mapping;

namespace TodoApp.Api.Controllers;

[ApiController]
[Route("api/todos")]
public class TodosController(ITodoService todoService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TodoResponse>>> GetAllTodos()
    {
        var todos = await todoService.GetAllAsync();
        var response = todos.Select(todo => todo.ToResponse());
        return Ok(response);        
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TodoResponse>> GetTodoById(Guid id)
    {
        var todo = await todoService.GetByIdAsync(id);
        return Ok(todo.ToResponse());
    }

    [HttpPost]
    public async Task<ActionResult<TodoResponse>> CreateTodo(CreateTodoRequest request)
    {
        var todo = await todoService.CreateAsync(request.Title);
        return CreatedAtAction(nameof(GetTodoById), new { id = todo.Id }, todo.ToResponse());
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateTodo(Guid id, UpdateTodoRequest request)
    {
        await todoService.UpdateAsync(id, request.Title);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTodo(Guid id)
    {
        await todoService.DeleteAsync(id);
        return NoContent();
    }

    [HttpPost("{id}/complete")]
    public async Task<ActionResult> CompleteTodo(Guid id)
    {
        await todoService.CompleteAsync(id);
        return NoContent();
    }

    [HttpPost("{id}/uncomplete")]
    public async Task<ActionResult> UncompleteTodo(Guid id)
    {
        await todoService.UncompleteAsync(id);
        return NoContent();
    }
}