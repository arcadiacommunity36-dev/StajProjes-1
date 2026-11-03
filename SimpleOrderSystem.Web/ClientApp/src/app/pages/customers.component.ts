import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { ApiService, errorMessage } from '../services/api.service';
import { Customer } from '../models';

@Component({
  selector: 'app-customers',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './customers.component.html'
})
export class CustomersComponent implements OnInit {
  customers: Customer[] = [];
  model = this.emptyModel();
  editId = 0;
  search = '';
  error = '';
  success = '';
  busy = false;
  loading = true;

  constructor(private api: ApiService) {}

  ngOnInit(): void { this.load(); }

  emptyModel() { return { name: '', code: '', address: '', phoneNumber: '', email: '', status: true }; }

  get filteredCustomers(): Customer[] {
    const search = this.search.trim().toLowerCase();
    return this.customers.filter(customer => (customer.name + ' ' + customer.code + ' ' + customer.email).toLowerCase().includes(search));
  }

  load(): void {
    this.loading = true;
    this.api.get<Customer[]>('/customers').subscribe({
      next: data => { this.customers = data; this.loading = false; },
      error: error => { this.error = errorMessage(error); this.loading = false; }
    });
  }

  edit(customer: Customer): void {
    this.editId = customer.id;
    this.model = { name: customer.name, code: customer.code, address: customer.address, phoneNumber: customer.phoneNumber, email: customer.email, status: customer.status };
    this.error = '';
    this.success = '';
  }

  reset(form?: NgForm): void {
    this.editId = 0;
    this.model = this.emptyModel();
    form?.resetForm(this.model);
  }

  save(form: NgForm): void {
    if (form.invalid || this.busy) return;
    this.busy = true;
    this.error = '';
    this.success = '';
    const request = this.editId ? this.api.put('/customers', this.editId, this.model) : this.api.post('/customers', this.model);
    request.subscribe({
      next: () => { this.busy = false; this.success = 'Customer saved.'; this.reset(form); this.load(); },
      error: error => { this.busy = false; this.error = errorMessage(error); }
    });
  }

  remove(customer: Customer): void {
    if (this.busy || !window.confirm('Delete customer "' + customer.name + '"?')) return;
    this.busy = true;
    this.error = '';
    this.success = '';
    this.api.delete('/customers', customer.id).subscribe({
      next: () => { this.busy = false; this.success = 'Customer deleted.'; this.reset(); this.load(); },
      error: error => { this.busy = false; this.error = errorMessage(error); }
    });
  }
}
