import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { ApiService, errorMessage } from '../services/api.service';
import { AuthService } from '../services/auth.service';

interface StockItem {
  id: number;
  productId: number;
  productName: string;
  colorName: string;
  size: string;
  quantity: number;
  isReadyForSale: boolean;
  canSell: boolean;
  status: boolean;
  rowVersion: string;
}

interface ProductOption {
  id: number;
  name: string;
  colorName: string;
  size: string;
}

@Component({
  selector: 'app-stock',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './stock.component.html'
})
export class StockComponent implements OnInit {
  items: StockItem[] = [];
  products: ProductOption[] = [];
  search = '';
  loading = true;
  saving = false;
  error = '';
  success = '';
  editingId = 0;
  model = { productId: 0, quantity: 0, isReadyForSale: false, status: true, rowVersion: '' };

  constructor(private api: ApiService, public auth: AuthService) {}

  ngOnInit(): void {
    this.load();
  }

  get filteredItems(): StockItem[] {
    const text = this.search.trim().toLowerCase();
    return this.items.filter(item => `${item.productName} ${item.colorName} ${item.size}`.toLowerCase().includes(text));
  }

  get totalQuantity(): number {
    return this.filteredItems.reduce((total, item) => total + item.quantity, 0);
  }

  load(): void {
    this.reset();
    this.loading = true;
    this.error = '';
    forkJoin({ items: this.api.get<StockItem[]>('/stock'), products: this.api.get<ProductOption[]>('/products?activeOnly=true') }).subscribe({
      next: result => {
        this.items = result.items;
        this.products = result.products;
        this.loading = false;
      },
      error: error => {
        this.error = errorMessage(error);
        this.loading = false;
      }
    });
  }

  edit(item: StockItem): void {
    this.editingId = item.id;
    this.model = { productId: item.productId, quantity: item.quantity, isReadyForSale: item.isReadyForSale, status: item.status, rowVersion: item.rowVersion };
    this.error = '';
    this.success = '';
  }

  reset(): void {
    this.editingId = 0;
    this.model = { productId: 0, quantity: 0, isReadyForSale: false, status: true, rowVersion: '' };
  }

  save(): void {
    if (this.saving || !this.auth.hasRole('Admin')) return;
    this.error = '';
    this.success = '';
    if (!this.model.productId || !Number.isInteger(this.model.quantity) || this.model.quantity < 0 || this.model.quantity > 2147483647) {
      this.error = 'Choose a product and enter a valid whole-number quantity.';
      return;
    }
    this.saving = true;
    const request = this.editingId
      ? this.api.put<void>('/stock', this.editingId, this.model)
      : this.api.post<void>('/stock', this.model);
    request.subscribe({
      next: () => {
        this.saving = false;
        this.success = 'Stock saved.';
        this.reset();
        this.load();
      },
      error: error => {
        this.error = errorMessage(error);
        this.saving = false;
      }
    });
  }

  remove(item: StockItem): void {
    if (this.saving || !this.auth.hasRole('Admin') || !window.confirm(`Delete the stock record for ${item.productName}?`)) return;
    this.saving = true;
    this.error = '';
    this.success = '';
    this.api.delete<void>('/stock', item.id).subscribe({
      next: () => {
        this.saving = false;
        this.success = 'Stock record deleted.';
        if (this.editingId === item.id) this.reset();
        this.load();
      },
      error: error => {
        this.error = errorMessage(error);
        this.saving = false;
      }
    });
  }
}
