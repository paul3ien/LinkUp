// T052: Authentication service – JWT stored in localStorage
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';

export interface LoginPayload { email: string; password: string; }
export interface LoginResponse { token: string; userId: string; email: string; username: string; }

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly AUTH_URL = 'http://localhost:7000';
  private readonly TOKEN_KEY    = 'lu_token';
  private readonly USER_KEY     = 'lu_user';
  private readonly EMAIL_KEY    = 'lu_email';
  private readonly USERNAME_KEY = 'lu_username';

  register(email: string, password: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.AUTH_URL}/api/auth/register`, { email, password }).pipe(
      tap(res => this.storeSession(res, email))
    );
  }

  login(email: string, password: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.AUTH_URL}/api/auth/login`, { email, password }).pipe(
      tap(res => this.storeSession(res, email))
    );
  }

  changeUsername(username: string): Observable<{ username: string }> {
    return this.http.put<{ username: string }>(`${this.AUTH_URL}/api/auth/profile/username`, { username }).pipe(
      tap(res => localStorage.setItem(this.USERNAME_KEY, res.username))
    );
  }

  changePassword(currentPassword: string, newPassword: string): Observable<void> {
    return this.http.put<void>(`${this.AUTH_URL}/api/auth/profile/password`, { currentPassword, newPassword });
  }

  logout(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
    localStorage.removeItem(this.EMAIL_KEY);
    localStorage.removeItem(this.USERNAME_KEY);
    this.router.navigate(['/login']);
  }

  getToken(): string | null    { return localStorage.getItem(this.TOKEN_KEY); }
  getUserId(): string | null   { return localStorage.getItem(this.USER_KEY); }
  getEmail(): string | null    { return localStorage.getItem(this.EMAIL_KEY); }
  getUsername(): string | null { return localStorage.getItem(this.USERNAME_KEY); }
  isLoggedIn(): boolean        { return !!this.getToken(); }

  private storeSession(res: LoginResponse, email: string): void {
    localStorage.setItem(this.TOKEN_KEY, res.token);
    localStorage.setItem(this.USER_KEY, res.userId);
    localStorage.setItem(this.EMAIL_KEY, email);
    localStorage.setItem(this.USERNAME_KEY, res.username ?? '');
  }
}

