using Microsoft.EntityFrameworkCore;
using TodoApp.Core;

namespace TodoApp.Infrastructure.Services;

public class TodoService(AppDbContext context) : ITodoService
{
    public async Task<TodoItem> GetByIdAsync(Guid id)
    {
        var todoItem = await context.TodoItems.FindAsync(id);
        return todoItem ?? throw new KeyNotFoundException($"Todo with ID {id} not found.");
    }

    public async Task<IEnumerable<TodoItem>> GetAllAsync()
    {
        return await context.TodoItems.ToListAsync();
    }

    public async Task<TodoItem> CreateAsync(string title)
    {
        var todoItem = new TodoItem(title);
        context.TodoItems.Add(todoItem);
        await context.SaveChangesAsync();
        return todoItem;
    }

    public async Task UpdateAsync(Guid id, string title)
    {
        var todoItem = await context.TodoItems.FindAsync(id);
        if (todoItem is null) return;

        todoItem.UpdateTitle(title);
        await context.SaveChangesAsync();
    }

    public async Task CompleteAsync(Guid id)
    {
        var todoItem = await context.TodoItems.FindAsync(id);
        if (todoItem is null) return;

        todoItem.Complete();
        await context.SaveChangesAsync();
    }

    public async Task UncompleteAsync(Guid id)
    {
        var todoItem = await context.TodoItems.FindAsync(id);
        if (todoItem is null) return;

        todoItem.Uncomplete();
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var todoItem = await context.TodoItems.FindAsync(id);
        if (todoItem is null) return;

        context.TodoItems.Remove(todoItem);
        await context.SaveChangesAsync();
    }

    public async Task<IEnumerable<TodoItem>> GetCompletedAsync()
    {
        return await context.TodoItems.Where(todo => todo.IsCompleted).ToListAsync();
    }

    public async Task<IEnumerable<TodoItem>> GetIncompleteAsync()
    {
        return await context.TodoItems.Where(todo => !todo.IsCompleted).ToListAsync();
    }
}