import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { ApiService, errorMessage } from '../services/api.service';
import { Role } from '../models';

@Component({
  selector: 'app-roles',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './roles.component.html'
})
export class RolesComponent implements OnInit {
  roles: Role[] = [];
  model = { name: '', status: true };
  editId = 0;
  error = '';
  success = '';
  busy = false;
  loading = true;
  systemNames = ['Admin', 'User', 'CustomerRepresentative'];

  constructor(private api: ApiService) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading = true;
    this.api.get<Role[]>('/roles').subscribe({
      next: data => { this.roles = data; this.loading = false; },
      error: error => { this.error = errorMessage(error); this.loading = false; }
    });
  }

  edit(role: Role): void {
    this.editId = role.id;
    this.model = { name: role.name, status: role.status };
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
    this.error = '';
    this.success = '';
    const request = this.editId ? this.api.put('/roles', this.editId, this.model) : this.api.post('/roles', this.model);
    request.subscribe({
      next: () => { this.busy = false; this.success = 'Role saved.'; this.reset(form); this.load(); },
      error: error => { this.busy = false; this.error = errorMessage(error); }
    });
  }

  remove(role: Role): void {
    if (this.busy || !window.confirm('Delete role "' + role.name + '"?')) return;
    this.busy = true;
    this.error = '';
    this.success = '';
    this.api.delete('/roles', role.id).subscribe({
      next: () => { this.busy = false; this.success = 'Role deleted.'; this.reset(); this.load(); },
      error: error => { this.busy = false; this.error = errorMessage(error); }
    });
  }
}
