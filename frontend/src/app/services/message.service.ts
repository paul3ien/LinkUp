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
      tap(messages => this.messages$.next(messages))
    );
  }

  createMessage(channelId: string, data: CreateMessageDto): Observable<Message> {
    return this.http.post<Message>(`${this.API_URL}/${channelId}/messages`, data).pipe(
      tap(message => {
        const current = this.messages$.value;
        this.messages$.next([...current, message]);
      })
    );
  }

  clearMessages(): void {
    this.messages$.next([]);
  }

  getMessages(): Message[] {
    return this.messages$.value;
  }
}
