import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Todo } from '../models/todo.model';

@Injectable({ providedIn: 'root' })
export class TodoService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = 'http://localhost:5118/api/todos';

  getAll(): Observable<Todo[]> {
    return this.http.get<Todo[]>(this.baseUrl);
  }

  create(title: string): Observable<Todo> {
    return this.http.post<Todo>(this.baseUrl, { title });
  }

  update(id: string, title: string): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, { title });
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  complete(id: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/complete`, null);
  }

  uncomplete(id: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/uncomplete`, null);
  }
}
