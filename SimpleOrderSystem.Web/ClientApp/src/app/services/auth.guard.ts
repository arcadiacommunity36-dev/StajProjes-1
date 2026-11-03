import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { catchError, map, of } from 'rxjs';
import { AuthService } from './auth.service';

export const authGuard: CanActivateFn = (route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  if (!auth.token) {
    return router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
  }
  return auth.refresh().pipe(
    map(() => {
      const roles = route.data['roles'] as string[] | undefined;
      if (roles && !auth.hasAnyRole(roles)) {
        return router.createUrlTree(['/forbidden']);
      }
      return true;
    }),
    catchError(() => {
      auth.logout();
      return of(router.createUrlTree(['/login']));
    })
  );
};
