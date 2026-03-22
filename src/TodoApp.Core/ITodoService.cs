namespace TodoApp.Core;

public interface ITodoService
{
    Task<TodoItem> GetByIdAsync(Guid id);
    Task<IEnumerable<TodoItem>> GetAllAsync();
    Task<TodoItem> CreateAsync(string title);
    Task UpdateAsync(Guid id, string title);
    Task CompleteAsync(Guid id);
    Task UncompleteAsync(Guid id);
    Task DeleteAsync(Guid id);
    Task<IEnumerable<TodoItem>> GetCompletedAsync();
    Task<IEnumerable<TodoItem>> GetIncompleteAsync();
}