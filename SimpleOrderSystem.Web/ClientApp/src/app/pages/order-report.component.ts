import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService, errorMessage } from '../services/api.service';

interface OrderRow {
  id: number;
  customerName: string;
  productName: string;
  colorName: string;
  size: string;
  quantity: number;
  createdDate: string;
}

@Component({
  selector: 'app-order-report',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './order-report.component.html'
})
export class OrderReportComponent implements OnInit {
  orders: OrderRow[] = [];
  search = '';
  loading = true;
  error = '';

  constructor(private api: ApiService) {}

  ngOnInit(): void {
    this.load();
  }

  get filteredOrders(): OrderRow[] {
    const text = this.search.trim().toLowerCase();
    return this.orders.filter(order => `${order.id} ${order.customerName} ${order.productName} ${order.colorName} ${order.size}`.toLowerCase().includes(text));
  }

  get totalQuantity(): number {
    return this.filteredOrders.reduce((total, order) => total + order.quantity, 0);
  }

  load(): void {
    this.loading = true;
    this.error = '';
    this.api.get<OrderRow[]>('/orders').subscribe({
      next: orders => {
        this.orders = orders;
        this.loading = false;
      },
      error: error => {
        this.error = errorMessage(error);
        this.loading = false;
      }
    });
  }
}
