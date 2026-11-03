import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { ApiService, errorMessage } from '../services/api.service';
import { Employee, Role } from '../models';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './users.component.html'
})
export class UsersComponent implements OnInit {
  users: Employee[] = [];
  roles: Role[] = [];
  model = this.emptyModel();
  editId = 0;
  search = '';
  error = '';
  success = '';
  busy = false;
  loading = true;

  constructor(private api: ApiService) {}

  ngOnInit(): void { this.load(); }

  emptyModel() {
    return { fullName: '', phoneNumber: '', email: '', gender: 'Other', password: '', roleIds: [] as number[], status: true };
  }

  get filteredUsers(): Employee[] {
    const search = this.search.trim().toLowerCase();
    return this.users.filter(user => (user.fullName + ' ' + user.email + ' ' + user.roles.join(' ')).toLowerCase().includes(search));
  }

  load(): void {
    this.loading = true;
    forkJoin({ users: this.api.get<Employee[]>('/users'), roles: this.api.get<Role[]>('/roles') }).subscribe({
      next: data => { this.users = data.users; this.roles = data.roles; this.loading = false; },
      error: error => { this.error = errorMessage(error); this.loading = false; }
    });
  }

  toggleRole(roleId: number, event: Event): void {
    const checked = (event.target as HTMLInputElement).checked;
    this.model.roleIds = checked ? [...this.model.roleIds, roleId] : this.model.roleIds.filter(id => id !== roleId);
  }

  edit(user: Employee): void {
    this.editId = user.id;
    this.model = { fullName: user.fullName, phoneNumber: user.phoneNumber, email: user.email, gender: user.gender, password: '', roleIds: [...user.roleIds], status: user.status };
    this.error = '';
    this.success = '';
  }

  reset(form?: NgForm): void {
    this.editId = 0;
    this.model = this.emptyModel();
    form?.resetForm(this.model);
  }

  save(form: NgForm): void {
    if (form.invalid || this.busy || this.model.roleIds.length === 0) return;
    this.busy = true;
    this.error = '';
    this.success = '';
    const request = this.editId ? this.api.put('/users', this.editId, this.model) : this.api.post('/users', this.model);
    request.subscribe({
      next: () => { this.busy = false; this.success = 'Employee saved.'; this.reset(form); this.load(); },
      error: error => { this.busy = false; this.error = errorMessage(error); }
    });
  }

  remove(user: Employee): void {
    if (this.busy || !window.confirm('Delete employee "' + user.fullName + '"?')) return;
    this.busy = true;
    this.error = '';
    this.success = '';
    this.api.delete('/users', user.id).subscribe({
      next: () => { this.busy = false; this.success = 'Employee deleted.'; this.reset(); this.load(); },
      error: error => { this.busy = false; this.error = errorMessage(error); }
    });
  }
}
