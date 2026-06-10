// T055/T071: ChatWindow – displays messages from BehaviorSubject stream (AsyncPipe)
import { Component, inject, ViewChild, ElementRef, AfterViewChecked, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ChannelService } from '../../services/channel.service';
import { MessageService } from '../../services/message.service';
import { AuthService } from '../../auth/auth.service';
import { ChatService } from '../chat.service';
import { UserCacheService } from '../../services/user-cache.service';
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
  private readonly userCache = inject(UserCacheService);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly destroy$ = new Subject<void>();
  @ViewChild('bottom') private bottom!: ElementRef;

  currentChannel$: Observable<Channel | null> = this.channelService.selectedChannel;
  messages$ = this.messageService.messages;
  draft = '';
  /** userId → resolved display name */
  userNames: Record<string, string> = {};

  ngOnInit(): void {
    // Pre-seed own name from localStorage (always available immediately)
    const myId = this.auth.getUserId();
    const myName = this.auth.getUsername();
    if (myId && myName) {
      this.userCache.set(myId, myName);
      this.userNames[myId] = myName;
    }

    // Resolve usernames for every message batch (REST + real-time gRPC)
    this.messageService.messages.pipe(takeUntil(this.destroy$)).subscribe(msgs => {
      this.resolveUsernames([...new Set(msgs.map(m => m.userId))]);
    });

    this.currentChannel$.pipe(takeUntil(this.destroy$)).subscribe(channel => {
      if (channel) {
        this.messageService.getMessagesByChannelId(channel.id).subscribe({
          next: msgs => {
            console.log('✅ Messages chargés:', msgs.length);
          },
          error: err => console.error('❌ Erreur chargement messages:', err)
        });
        this.chatService.joinChannel(channel.id);
      } else {
        this.messageService.clearMessages();
        this.chatService.leaveChannel();
      }
    });
  }

  private resolveUsernames(userIds: string[]): void {
    for (const id of userIds) {
      if (this.userNames[id]) continue;
      this.userCache.getUsernameAsync(id).subscribe(name => {
        this.userNames = { ...this.userNames, [id]: name };
        this.cdr.markForCheck();
      });
    }
  }

  displayName(userId: string): string {
    return this.userNames[userId] ?? userId.slice(0, 8) + '…';
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
    let currentChannelId: string | null = null;
    this.currentChannel$.subscribe(channel => {
      currentChannelId = channel?.id || null;
    }).unsubscribe();

    if (!this.draft.trim() || !currentChannelId) return;
    const body: SendMessageDto = { content: this.draft };
    this.draft = '';
    this.messageService.createMessage(currentChannelId, body).subscribe({
      next: msg => {
        console.log('✅ Message envoyé:', msg);
        this.resolveUsernames([msg.userId]);
      },
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
