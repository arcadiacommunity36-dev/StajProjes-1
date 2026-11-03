import { Routes } from '@angular/router';
import { authGuard } from './services/auth.guard';
import { LoginComponent } from './pages/login.component';
import { UsersComponent } from './pages/users.component';
import { RolesComponent } from './pages/roles.component';
import { CustomersComponent } from './pages/customers.component';
import { ProductsComponent } from './pages/products.component';
import { StockComponent } from './pages/stock.component';
import { OrdersComponent } from './pages/orders.component';
import { OrderReportComponent } from './pages/order-report.component';
import { ForbiddenComponent } from './pages/forbidden.component';

const allRoles = ['Admin', 'User', 'CustomerRepresentative'];

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'users', component: UsersComponent, canActivate: [authGuard], data: { roles: ['Admin'] } },
  { path: 'roles', component: RolesComponent, canActivate: [authGuard], data: { roles: ['Admin'] } },
  { path: 'customers', component: CustomersComponent, canActivate: [authGuard], data: { roles: ['Admin', 'CustomerRepresentative'] } },
  { path: 'products', component: ProductsComponent, canActivate: [authGuard], data: { roles: allRoles } },
  { path: 'orders', component: OrdersComponent, canActivate: [authGuard], data: { roles: ['CustomerRepresentative'] } },
  { path: 'stock', component: StockComponent, canActivate: [authGuard], data: { roles: allRoles } },
  { path: 'order-report', component: OrderReportComponent, canActivate: [authGuard], data: { roles: allRoles } },
  { path: 'forbidden', component: ForbiddenComponent, canActivate: [authGuard] },
  { path: '', redirectTo: 'stock', pathMatch: 'full' },
  { path: '**', redirectTo: 'stock' }
];
