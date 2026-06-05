import { Pipe, PipeTransform, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { UserCacheService } from '../services/user-cache.service';

@Pipe({ name: 'username', standalone: true, pure: false })
export class UsernamePipe implements PipeTransform {
  private readonly cache = inject(UserCacheService);
  private resolved = new Map<string, string>();

  transform(userId: string): string {
    const cached = this.cache.get(userId);
    if (cached) return cached;

    // If not yet in cache, trigger fetch and return short fallback
    if (!this.resolved.has(userId)) {
      this.resolved.set(userId, '');
      this.cache.getUsernameAsync(userId).subscribe(name => {
        this.resolved.set(userId, name);
      });
    }
    return userId.slice(0, 8) + '…';
  }
}
