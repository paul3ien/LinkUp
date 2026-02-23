// T071: Channel Service - Manage channels CRUD
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { tap } from 'rxjs/operators';

export interface Channel {
  id: string;
  name: string;
  description?: string;
  createdAt?: string;
}

export interface CreateChannelDto {
  name: string;
  description?: string;
}

@Injectable({ providedIn: 'root' })
export class ChannelService {
  private readonly http = inject(HttpClient);
  private readonly API_URL = 'http://localhost:7001/api/channels';
  
  private channels$ = new BehaviorSubject<Channel[]>([]);
  private selectedChannel$ = new BehaviorSubject<Channel | null>(null);

  channels = this.channels$.asObservable();
  selectedChannel = this.selectedChannel$.asObservable();

  getAllChannels(): Observable<Channel[]> {
    return this.http.get<Channel[]>(this.API_URL).pipe(
      tap(channels => this.channels$.next(channels))
    );
  }

  getChannelById(id: string): Observable<Channel> {
    return this.http.get<Channel>(`${this.API_URL}/${id}`);
  }

  createChannel(data: CreateChannelDto): Observable<Channel> {
    return this.http.post<Channel>(this.API_URL, data).pipe(
      tap(channel => {
        const current = this.channels$.value;
        this.channels$.next([...current, channel]);
      })
    );
  }

  selectChannel(channel: Channel): void {
    this.selectedChannel$.next(channel);
  }

  getSelectedChannel(): Channel | null {
    return this.selectedChannel$.value;
  }

  clearSelection(): void {
    this.selectedChannel$.next(null);
  }
}
