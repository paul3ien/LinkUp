// T055/T071: ChatWindow – displays messages from BehaviorSubject stream (AsyncPipe)
import { Component, inject, ViewChild, ElementRef, AfterViewChecked, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ChannelService } from '../../services/channel.service';
import { MessageService } from '../../services/message.service';
import { AuthService } from '../../auth/auth.service';
import { Observable } from 'rxjs';
import { Channel } from '../../services/channel.service';

export interface SendMessageDto { content: string; }

@Component({
  selector: 'app-chat-window',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './chat-window.component.html',
  styleUrl: './chat-window.component.css'
})
export class ChatWindowComponent implements OnInit, AfterViewChecked {
  private readonly channelService = inject(ChannelService);
  private readonly messageService = inject(MessageService);
  private readonly auth = inject(AuthService);
  @ViewChild('bottom') private bottom!: ElementRef;

  currentChannel$: Observable<Channel | null> = this.channelService.selectedChannel;
  messages$ = this.messageService.messages;
  draft = '';

  ngOnInit(): void {
    // Load messages when channel changes
    this.currentChannel$.subscribe(channel => {
      if (channel) {
        this.messageService.getMessagesByChannelId(channel.id);
      } else {
        this.messageService.clearMessages();
      }
    });
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
    this.messageService.createMessage(currentChannelId, body);
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
