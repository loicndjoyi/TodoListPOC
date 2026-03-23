import {
  Component,
  ChangeDetectionStrategy,
  input,
  output,
  OnChanges,
  SimpleChanges,
} from '@angular/core';
import { ReactiveFormsModule, FormGroup, FormControl, Validators } from '@angular/forms';


@Component({
  selector: 'app-todo-form',
  imports: [ReactiveFormsModule],
  templateUrl: './todo-form.component.html',
  styleUrl: './todo-form.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TodoFormComponent implements OnChanges {
  readonly initialTitle = input('');
  readonly showFeedback = input(true);
  readonly save = output<string>();
  readonly cancel = output<void>();

  readonly form = new FormGroup({
    title: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(200)] }),
  });

  get titleControl() {
    return this.form.controls.title;
  }

  get remainingChars(): number {
    return 200 - this.titleControl.value.length;
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['initialTitle']) {
      this.titleControl.setValue(this.initialTitle());
    }
  }

  onSubmit(): void {
    if (this.form.invalid) return;
    this.save.emit(this.titleControl.value.trim());
    this.form.reset();
  }

  onCancel(): void {
    this.cancel.emit();
    this.form.reset();
  }
}
