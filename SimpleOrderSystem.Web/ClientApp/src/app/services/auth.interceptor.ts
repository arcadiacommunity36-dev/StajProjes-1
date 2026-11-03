import { inject } from '@angular/core';
import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';
import { AuthService } from './auth.service';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const auth = inject(AuthService);
  const apiRequest = request.url.startsWith('/api/');
  const token = auth.token;
  if (apiRequest && token) {
    request = request.clone({ setHeaders: { Authorization: 'Bearer ' + token } });
  }
  return next(request).pipe(
    catchError((error: HttpErrorResponse) => {
      if (apiRequest && error.status === 401 && !request.url.endsWith('/auth/login')) {
        auth.logout();
      }
      return throwError(() => error);
    })
  );
};
