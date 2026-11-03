import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { ApiService, errorMessage } from '../services/api.service';
import { Color } from '../models';

@Component({
  selector: 'app-colors-dialog',
  standalone: true,
  imports: [CommonModule, FormsModule, MatDialogModule],
  templateUrl: './colors-dialog.component.html'
})
export class ColorsDialogComponent implements OnInit {
  colors: Color[] = [];
  model = { name: '', status: true };
  editId = 0;
  error = '';
  success = '';
  busy = false;
  loading = true;

  constructor(private api: ApiService, public dialog: MatDialogRef<ColorsDialogComponent>) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading = true;
    this.api.get<Color[]>('/colors').subscribe({
      next: data => { this.colors = data; this.loading = false; },
      error: error => { this.error = errorMessage(error); this.loading = false; }
    });
  }

  edit(color: Color): void {
    this.editId = color.id;
    this.model = { name: color.name, status: color.status };
    this.error = '';
    this.success = '';
  }

  reset(form?: NgForm): void {
    this.editId = 0;
    this.model = { name: '', status: true };
    form?.resetForm(this.model);
  }

  save(form: NgForm): void {
    if (form.invalid || this.busy) return;
    this.busy = true;
    this.dialog.disableClose = true;
    this.error = '';
    this.success = '';
    const request = this.editId ? this.api.put('/colors', this.editId, this.model) : this.api.post('/colors', this.model);
    request.subscribe({
      next: () => { this.finish(); this.success = 'Color saved.'; this.reset(form); this.load(); },
      error: error => { this.finish(); this.error = errorMessage(error); }
    });
  }

  remove(color: Color): void {
    if (this.busy || !window.confirm('Delete color "' + color.name + '"?')) return;
    this.busy = true;
    this.dialog.disableClose = true;
    this.error = '';
    this.success = '';
    this.api.delete('/colors', color.id).subscribe({
      next: () => { this.finish(); this.success = 'Color deleted.'; this.reset(); this.load(); },
      error: error => { this.finish(); this.error = errorMessage(error); }
    });
  }

  finish(): void {
    this.busy = false;
    this.dialog.disableClose = false;
  }
}
