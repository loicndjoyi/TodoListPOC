namespace TodoApp.Core;

public interface ITaskService
{
    Task<TodoItem> GetTaskByIdAsync(Guid id);
    Task<IEnumerable<TodoItem>> GetAllTasksAsync();
    Task<TodoItem> CreateTaskAsync(string title);
    Task UpdateTaskAsync(Guid id, string title);
    Task CompleteTaskAsync(Guid id);
    Task UncompleteTaskAsync(Guid id);
    Task DeleteTaskAsync(Guid id);
    Task<IEnumerable<TodoItem>> GetCompletedTasksAsync();
    Task<IEnumerable<TodoItem>> GetIncompleteTasksAsync();
}