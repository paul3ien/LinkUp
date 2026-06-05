// T055/T071: ChatWindow – displays messages from BehaviorSubject stream (AsyncPipe)
import { Component, inject, ViewChild, ElementRef, AfterViewChecked, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ChannelService } from '../../services/channel.service';
import { MessageService } from '../../services/message.service';
import { AuthService } from '../../auth/auth.service';
import { ChatService } from '../chat.service';
import { Observable, Subject, takeUntil } from 'rxjs';
import { Channel } from '../../services/channel.service';

export interface SendMessageDto { content: string; }

@Component({
  selector: 'app-chat-window',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './chat-window.component.html',
  styleUrl: './chat-window.component.css'
})
export class ChatWindowComponent implements OnInit, AfterViewChecked, OnDestroy {
  private readonly channelService = inject(ChannelService);
  private readonly messageService = inject(MessageService);
  private readonly chatService = inject(ChatService);
  private readonly auth = inject(AuthService);
  private readonly destroy$ = new Subject<void>();
  @ViewChild('bottom') private bottom!: ElementRef;

  currentChannel$: Observable<Channel | null> = this.channelService.selectedChannel;
  messages$ = this.messageService.messages;
  draft = '';

  ngOnInit(): void {
    this.currentChannel$.pipe(takeUntil(this.destroy$)).subscribe(channel => {
      if (channel) {
        // Load history via REST
        this.messageService.getMessagesByChannelId(channel.id).subscribe({
          next: msgs => console.log('✅ Messages chargés:', msgs.length),
          error: err => console.error('❌ Erreur chargement messages:', err)
        });
        // Subscribe to real-time via gRPC-Web
        this.chatService.joinChannel(channel.id);
      } else {
        this.messageService.clearMessages();
        this.chatService.leaveChannel();
      }
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.chatService.leaveChannel();
  }

  ngAfterViewChecked(): void {
    this.bottom?.nativeElement?.scrollIntoView({ behavior: 'smooth' });
  }

  send(): void {
    // Get current channel from service
    let currentChannelId: string | null = null;
    this.currentChannel$.subscribe(channel => {
      currentChannelId = channel?.id || null;
    }).unsubscribe();

    if (!this.draft.trim() || !currentChannelId) return;
    const body: SendMessageDto = { content: this.draft };
    this.draft = '';
    this.messageService.createMessage(currentChannelId, body).subscribe({
      next: msg => console.log('✅ Message envoyé:', msg),
      error: err => console.error('❌ Erreur envoi message:', err)
    });
  }

  onKeydown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.send();
    }
  }

  logout(): void { this.auth.logout(); }
}
