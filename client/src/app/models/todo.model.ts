export interface Todo {
  id: string;
  title: string;
  isCompleted: boolean;
  createdAt: string;
  completedAt: string | null;
}
