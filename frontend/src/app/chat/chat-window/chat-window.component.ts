// T055: ChatWindow – displays messages from BehaviorSubject stream (AsyncPipe)
import { Component, inject, ViewChild, ElementRef, AfterViewChecked } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { ChatService } from '../chat.service';
import { AuthService } from '../../auth/auth.service';

export interface SendMessageDto { content: string; }

@Component({
  selector: 'app-chat-window',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './chat-window.component.html',
  styleUrl: './chat-window.component.css'
})
export class ChatWindowComponent implements AfterViewChecked {
  readonly chatService = inject(ChatService);
  private readonly http = inject(HttpClient);
  private readonly auth = inject(AuthService);
  @ViewChild('bottom') private bottom!: ElementRef;

  messages$ = this.chatService.messages$;
  draft = '';

  ngAfterViewChecked(): void {
    this.bottom?.nativeElement?.scrollIntoView({ behavior: 'smooth' });
  }

  send(): void {
    const channelId = this.chatService.currentChannelId;
    if (!this.draft.trim() || !channelId) return;
    const body: SendMessageDto = { content: this.draft };
    this.http.post(`http://localhost:5002/api/channels/${channelId}/messages`, body).subscribe();
    this.draft = '';
  }

  onKeydown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.send();
    }
  }

  logout(): void { this.auth.logout(); }
}
