import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';

@Injectable({ providedIn: 'root' })
export class ApiService {
  constructor(private http: HttpClient) {}

  get<T>(path: string) {
    return this.http.get<T>('/api' + path);
  }

  post<T>(path: string, value: unknown) {
    return this.http.post<T>('/api' + path, value);
  }

  put<T>(path: string, id: number, value: unknown) {
    return this.http.put<T>('/api' + path + '/' + id, value);
  }

  delete<T>(path: string, id: number) {
    return this.http.delete<T>('/api' + path + '/' + id);
  }
}

export function errorMessage(error: HttpErrorResponse): string {
  if (typeof error.error?.message === 'string') {
    return error.error.message;
  }
  if (error.error?.errors) {
    return Object.values(error.error.errors).flat().join(' ');
  }
  if (error.status === 0) {
    return 'The server could not be reached. Check that the application is running.';
  }
  if (error.status === 401) {
    return 'Your session has expired. Please sign in again.';
  }
  if (error.status === 403) {
    return 'You do not have permission to perform this action.';
  }
  return 'The operation could not be completed. Please try again.';
}
