import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { ApiService, errorMessage } from '../services/api.service';

interface CustomerOption {
  id: number;
  name: string;
  code: string;
}

interface ProductOption {
  id: number;
  name: string;
  colorName: string;
  size: string;
}

interface AvailableStock {
  productId: number;
  quantity: number;
  isReadyForSale: boolean;
  canSell: boolean;
  status: boolean;
}

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './orders.component.html'
})
export class OrdersComponent implements OnInit {
  customers: CustomerOption[] = [];
  products: ProductOption[] = [];
  stock: AvailableStock[] = [];
  model = { customerId: 0, productId: 0, quantity: 1 };
  loading = true;
  saving = false;
  error = '';
  success = '';

  constructor(private api: ApiService) {}

  ngOnInit(): void {
    this.load();
  }

  get selectedStock(): AvailableStock | undefined {
    return this.stock.find(item => item.productId === this.model.productId);
  }

  load(): void {
    this.loading = true;
    forkJoin({
      customers: this.api.get<CustomerOption[]>('/customers?activeOnly=true'),
      products: this.api.get<ProductOption[]>('/products?activeOnly=true'),
      stock: this.api.get<AvailableStock[]>('/stock')
    }).subscribe({
      next: result => {
        this.customers = result.customers;
        this.products = result.products;
        this.stock = result.stock;
        this.loading = false;
      },
      error: error => {
        this.error = errorMessage(error);
        this.loading = false;
      }
    });
  }

  save(): void {
    if (this.saving) return;
    this.error = '';
    this.success = '';
    if (!this.model.customerId || !this.model.productId || !Number.isInteger(this.model.quantity) || this.model.quantity < 1 || this.model.quantity > 2147483647) {
      this.error = 'Choose a customer and a product, then enter a positive whole-number quantity.';
      return;
    }
    this.saving = true;
    this.api.post<void>('/orders', this.model).subscribe({
      next: () => {
        this.saving = false;
        this.success = 'Order created. The ordered quantity has been deducted from stock.';
        this.model = { customerId: this.model.customerId, productId: 0, quantity: 1 };
        this.load();
      },
      error: error => {
        this.error = errorMessage(error);
        this.saving = false;
        this.api.get<AvailableStock[]>('/stock').subscribe({
          next: stock => this.stock = stock,
          error: () => this.stock = []
        });
      }
    });
  }
}
