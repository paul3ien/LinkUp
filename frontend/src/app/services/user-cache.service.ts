import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { tap } from 'rxjs/operators';

@Injectable({ providedIn: 'root' })
export class UserCacheService {
  private readonly http = inject(HttpClient);
  private readonly AUTH_URL = 'http://localhost:7000';
  private cache = new Map<string, string>(); // userId → username

  getUsername(userId: string): Observable<string> {
    if (this.cache.has(userId)) {
      return of(this.cache.get(userId)!);
    }
    return this.http.get<{ username: string }>(`${this.AUTH_URL}/api/auth/users/${userId}/username`).pipe(
      tap(res => this.cache.set(userId, res.username)),
      // map to just the string via further pipe in template – see below
    ) as unknown as Observable<string>;
  }

  getUsernameAsync(userId: string): Observable<string> {
    if (this.cache.has(userId)) {
      return of(this.cache.get(userId)!);
    }
    return new Observable(observer => {
      this.http.get<{ username: string }>(`${this.AUTH_URL}/api/auth/users/${userId}/username`).subscribe({
        next: res => {
          this.cache.set(userId, res.username);
          observer.next(res.username);
          observer.complete();
        },
        error: () => {
          observer.next(userId.slice(0, 8) + '…');
          observer.complete();
        }
      });
    });
  }

  /** Pre-warm cache for a list of userIds */
  prefetch(userIds: string[]): void {
    const missing = [...new Set(userIds)].filter(id => !this.cache.has(id));
    for (const id of missing) {
      this.http.get<{ username: string }>(`${this.AUTH_URL}/api/auth/users/${id}/username`).subscribe({
        next: res => this.cache.set(id, res.username),
        error: () => this.cache.set(id, id.slice(0, 8) + '…')
      });
    }
  }

  get(userId: string): string | undefined { return this.cache.get(userId); }
  set(userId: string, username: string): void { this.cache.set(userId, username); }
}
