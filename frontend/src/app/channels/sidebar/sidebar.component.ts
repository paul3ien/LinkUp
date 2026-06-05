// T054: Sidebar – lists channels from REST API, selects active channel
import { Component, inject, OnInit } from '@angular/core';

import { HttpClient } from '@angular/common/http';
import { ChatService } from '../../chat/chat.service';

export interface Channel { id: string; name: string; description: string; }

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.css'
})
export class SidebarComponent implements OnInit {
  private readonly http = inject(HttpClient);
  readonly chatService = inject(ChatService);

  channels: Channel[] = [];

  ngOnInit(): void {
    this.http.get<Channel[]>('http://localhost:5002/api/channels').subscribe({
      next: ch => {
        this.channels = ch;
        if (ch.length > 0) this.selectChannel(ch[0]);
      },
      error: () => { /* backend offline in dev – use mock */ this.channels = []; }
    });
  }

  selectChannel(ch: Channel): void {
    this.chatService.joinChannel(ch.id);
  }

  isActive(ch: Channel): boolean {
    return this.chatService.currentChannelId === ch.id;
  }
}
