import { Component, ChangeDetectionStrategy, inject, OnInit, signal, computed } from '@angular/core';
import { TodoService } from '../../services/todo.service';
import { Todo } from '../../models/todo.model';
import { TodoFormComponent } from '../todo-form/todo-form.component';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-todo-list',
  imports: [TodoFormComponent],
  templateUrl: './todo-list.component.html',
  styleUrl: './todo-list.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TodoListComponent implements OnInit {
  private readonly todoService = inject(TodoService);

  // --- State ---
  readonly todos = signal<Todo[]>([]);
  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly editingId = signal<string | null>(null);

  // --- Derived ---
  readonly completedCount = computed(() => this.todos().filter(t => t.isCompleted).length);
  readonly incompleteCount = computed(() => this.todos().filter(t => !t.isCompleted).length);

  ngOnInit(): void {
    this.loadAll();
  }

  loadAll(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.todoService.getAll().subscribe({
      next: todos => {
        this.todos.set(todos);
        this.isLoading.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.errorMessage.set('Could not load todos. Is the API running?');
        this.isLoading.set(false);
      },
    });
  }

  createTodo(title: string): void {
    this.todoService.create(title).subscribe({
      next: todo => this.todos.update(list => [...list, todo]),
      error: () => this.errorMessage.set('Failed to create todo.'),
    });
  }

  updateTodo(id: string, title: string): void {
    this.todoService.update(id, title).subscribe({
      next: () => {
        this.todos.update(list =>
          list.map(t => (t.id === id ? { ...t, title } : t))
        );
        this.editingId.set(null);
      },
      error: () => this.errorMessage.set('Failed to update todo.'),
    });
  }

  deleteTodo(id: string): void {
    this.todoService.delete(id).subscribe({
      next: () => this.todos.update(list => list.filter(t => t.id !== id)),
      error: () => this.errorMessage.set('Failed to delete todo.'),
    });
  }

  toggleComplete(todo: Todo): void {
    const action$ = todo.isCompleted
      ? this.todoService.uncomplete(todo.id)
      : this.todoService.complete(todo.id);

    action$.subscribe({
      next: () =>
        this.todos.update(list =>
          list.map(t =>
            t.id === todo.id ? { ...t, isCompleted: !t.isCompleted, completedAt: !t.isCompleted ? 'pending' : null } : t
          )
        ),
      error: () => this.errorMessage.set('Failed to update todo status.'),
    });
  }

  startEdit(id: string): void {
    this.editingId.set(id);
  }

  cancelEdit(): void {
    this.editingId.set(null);
  }

  dismissError(): void {
    this.errorMessage.set(null);
  }
}
