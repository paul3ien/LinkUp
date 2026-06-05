// T071: Message Service - Manage messages for channels
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { tap, map } from 'rxjs/operators';

export interface Message {
  id: string;
  channelId: string;
  userId: string;
  content: string;
  createdAt: string;
}

export interface CreateMessageDto {
  content: string;
}

@Injectable({ providedIn: 'root' })
export class MessageService {
  private readonly http = inject(HttpClient);
  private readonly API_URL = 'http://localhost:7001/api/channels';

  private messages$ = new BehaviorSubject<Message[]>([]);
  messages = this.messages$.asObservable();

  getMessagesByChannelId(channelId: string): Observable<Message[]> {
    return this.http.get<any>(`${this.API_URL}/${channelId}/messages`).pipe(
      map(response => Array.isArray(response) ? response : (response?.data ?? [])),
      map(messages => [...messages].sort((a, b) => new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime())),
      tap(messages => this.messages$.next(messages))
    );
  }

  addRealtimeMessage(msg: Message): void {
    const current = this.messages$.value;
    if (current.some(m => m.id === msg.id)) return; // dedup
    const sorted = [...current, msg].sort((a, b) => new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime());
    this.messages$.next(sorted);
  }

  createMessage(channelId: string, data: CreateMessageDto): Observable<Message> {
    return this.http.post<Message>(`${this.API_URL}/${channelId}/messages`, data).pipe(
      tap(message => this.addRealtimeMessage(message))
    );
  }

  clearMessages(): void {
    this.messages$.next([]);
  }

  getMessages(): Message[] {
    return this.messages$.value;
  }
}
