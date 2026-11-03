import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { errorMessage } from '../services/api.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html'
})
export class LoginComponent {
  email = '';
  password = '';
  error = '';
  busy = false;

  constructor(private auth: AuthService, private router: Router, private route: ActivatedRoute) {}

  submit(): void {
    if (this.busy) return;
    this.busy = true;
    this.error = '';
    this.auth.login(this.email.trim(), this.password).subscribe({
      next: () => {
        this.busy = false;
        const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl');
        const destination = returnUrl?.startsWith('/') && !returnUrl.startsWith('//') && returnUrl !== '/login' ? returnUrl : '/stock';
        void this.router.navigateByUrl(destination);
      },
      error: error => {
        this.busy = false;
        this.error = errorMessage(error);
      }
    });
  }
}
