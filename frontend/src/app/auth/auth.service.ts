// T052: Authentication service – JWT stored in localStorage
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';

export interface LoginPayload { email: string; password: string; }
export interface LoginResponse { token: string; userId: string; email: string; }

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly AUTH_URL = 'http://localhost:7000';
  private readonly TOKEN_KEY = 'lu_token';
  private readonly USER_KEY   = 'lu_user';
  private readonly EMAIL_KEY  = 'lu_email';

  register(email: string, password: string): Observable<any> {
    return this.http.post(`${this.AUTH_URL}/api/auth/register`, { email, password });
  }

  login(email: string, password: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.AUTH_URL}/api/auth/login`, { email, password }).pipe(
      tap(res => {
        localStorage.setItem(this.TOKEN_KEY, res.token);
        localStorage.setItem(this.USER_KEY, res.userId);
        localStorage.setItem(this.EMAIL_KEY, email);
      })
    );
  }

  logout(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
    localStorage.removeItem(this.EMAIL_KEY);
    this.router.navigate(['/login']);
  }

  getToken(): string | null { return localStorage.getItem(this.TOKEN_KEY); }
  getUserId(): string | null { return localStorage.getItem(this.USER_KEY); }
  getEmail(): string | null { return localStorage.getItem(this.EMAIL_KEY); }
  isLoggedIn(): boolean { return !!this.getToken(); }
}

