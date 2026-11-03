import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-forbidden',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `<section class="panel"><h1>Access denied</h1><p>Your assigned roles do not allow access to this page.</p><p class="muted">Contact an administrator if you need additional access.</p><div class="actions"><a *ngIf="auth.hasAnyRole(['Admin', 'User', 'CustomerRepresentative'])" routerLink="/stock" class="button">Go to stock report</a><button class="button secondary" (click)="auth.logout()">Sign out</button></div></section>`
})
export class ForbiddenComponent {
  constructor(public auth: AuthService) {}
}
