import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { forkJoin } from 'rxjs';
import { ApiService, errorMessage } from '../services/api.service';
import { Color, Product } from '../models';
import { ColorsDialogComponent } from './colors-dialog.component';

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [CommonModule, FormsModule, MatDialogModule],
  templateUrl: './products.component.html'
})
export class ProductsComponent implements OnInit {
  products: Product[] = [];
  colors: Color[] = [];
  sizes = ['S', 'M', 'L', 'XL'];
  model = this.emptyModel();
  editId = 0;
  search = '';
  error = '';
  success = '';
  busy = false;
  loading = true;

  constructor(private api: ApiService, private dialog: MatDialog) {}

  ngOnInit(): void { this.load(); }

  emptyModel() { return { name: '', colorId: 0, size: 'M', status: true }; }

  get filteredProducts(): Product[] {
    const search = this.search.trim().toLowerCase();
    return this.products.filter(product => (product.name + ' ' + product.colorName + ' ' + product.size).toLowerCase().includes(search));
  }

  load(): void {
    this.loading = true;
    forkJoin({ products: this.api.get<Product[]>('/products'), colors: this.api.get<Color[]>('/colors') }).subscribe({
      next: data => { this.products = data.products; this.colors = data.colors; this.loading = false; },
      error: error => { this.error = errorMessage(error); this.loading = false; }
    });
  }

  openColors(): void {
    this.dialog.open(ColorsDialogComponent, { width: '680px', maxWidth: '95vw', ariaLabel: 'Manage product colors' }).afterClosed().subscribe(() => this.load());
  }

  edit(product: Product): void {
    this.editId = product.id;
    this.model = { name: product.name, colorId: product.colorId, size: product.size, status: product.status };
    this.error = '';
    this.success = '';
  }

  reset(form?: NgForm): void {
    this.editId = 0;
    this.model = this.emptyModel();
    form?.resetForm(this.model);
  }

  save(form: NgForm): void {
    if (form.invalid || this.busy || !this.model.colorId) return;
    this.busy = true;
    this.error = '';
    this.success = '';
    const request = this.editId ? this.api.put('/products', this.editId, this.model) : this.api.post('/products', this.model);
    request.subscribe({
      next: () => { this.busy = false; this.success = 'Product saved.'; this.reset(form); this.load(); },
      error: error => { this.busy = false; this.error = errorMessage(error); }
    });
  }

  remove(product: Product): void {
    if (this.busy || !window.confirm('Delete product "' + product.name + '"?')) return;
    this.busy = true;
    this.error = '';
    this.success = '';
    this.api.delete('/products', product.id).subscribe({
      next: () => { this.busy = false; this.success = 'Product deleted.'; this.reset(); this.load(); },
      error: error => { this.busy = false; this.error = errorMessage(error); }
    });
  }
}
