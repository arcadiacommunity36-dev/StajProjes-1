import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { tap } from 'rxjs';
import { ApiService } from './api.service';
import { CurrentUser, LoginResponse } from '../models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  user: CurrentUser | null = null;

  constructor(private api: ApiService, private router: Router) {}

  get token(): string | null {
    return sessionStorage.getItem('orderToken');
  }

  login(email: string, password: string) {
    return this.api.post<LoginResponse>('/auth/login', { email, password }).pipe(
      tap(result => {
        sessionStorage.setItem('orderToken', result.token);
        this.user = result.user;
      })
    );
  }

  refresh() {
    return this.api.get<CurrentUser>('/auth/me').pipe(tap(user => this.user = user));
  }

  hasRole(role: string): boolean {
    return this.user?.roles.includes(role) ?? false;
  }

  hasAnyRole(roles: string[]): boolean {
    return roles.some(role => this.hasRole(role));
  }

  logout(): void {
    sessionStorage.removeItem('orderToken');
    this.user = null;
    void this.router.navigate(['/login']);
  }
}
