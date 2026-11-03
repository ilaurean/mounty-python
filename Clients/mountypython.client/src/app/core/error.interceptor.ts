import { HttpContextToken, HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { catchError, throwError } from 'rxjs';

export const HANDLE_HTTP_ERROR_LOCALLY = new HttpContextToken<boolean>(() => false);

export function describeError(error: HttpErrorResponse): string {
  const body = error.error;
  if (body?.errors && typeof body.errors === 'object') {
    return Object.values(body.errors as Record<string, string[]>).flat().join(' ');
  }
  return body?.detail ?? body?.title ?? (error.status === 403 ? 'You are not allowed to do this.' : error.message);
}

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const snackBar = inject(MatSnackBar);
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status !== 401 && !req.context.get(HANDLE_HTTP_ERROR_LOCALLY)) {
        snackBar.open(describeError(error), 'Close', { duration: 6000 });
      }
      return throwError(() => error);
    })
  );
};
