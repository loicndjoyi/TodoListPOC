using Microsoft.EntityFrameworkCore;
using TodoApp.Core;

namespace TodoApp.Infrastructure.Services;

public class TaskService : ITaskService
{
    private readonly AppDbContext _context;

    public TaskService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TodoItem> GetTaskByIdAsync(Guid id)
    {
        var todoItem = await _context.TodoItems.FindAsync(id);
        if (todoItem == null)
            throw new KeyNotFoundException($"Task with ID {id} not found.");
        return todoItem;
    }

    public async Task<IEnumerable<TodoItem>> GetAllTasksAsync()
    {
        return await _context.TodoItems.ToListAsync();
    }

    public async Task<TodoItem> CreateTaskAsync(string title)
    {
        var todoItem = new TodoItem(title);
        _context.TodoItems.Add(todoItem);
        await _context.SaveChangesAsync();
        return todoItem;
    }

    public async Task UpdateTaskAsync(Guid id, string title)
    {
        var task = await _context.TodoItems.FindAsync(id);
        if (task != null)
        {
            task.UpdateTitle(title);
            await _context.SaveChangesAsync();
        }
    }

    public async Task CompleteTaskAsync(Guid id)
    {
        var task = await _context.TodoItems.FindAsync(id);
        if (task != null)
        {
            task.Complete();
            await _context.SaveChangesAsync();
        }
    }

    public async Task UncompleteTaskAsync(Guid id)
    {
        var task = await _context.TodoItems.FindAsync(id);
        if (task != null)
        {
            task.Uncomplete();
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteTaskAsync(Guid id)
    {
        var todoItem = await _context.TodoItems.FindAsync(id);
        if (todoItem != null)
        {
            _context.TodoItems.Remove(todoItem);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<TodoItem>> GetCompletedTasksAsync()
    {
        return await _context.TodoItems.Where(t => t.IsCompleted).ToListAsync();
    }

    public async Task<IEnumerable<TodoItem>> GetIncompleteTasksAsync()
    {
        return await _context.TodoItems.Where(t => !t.IsCompleted).ToListAsync();
    }
}