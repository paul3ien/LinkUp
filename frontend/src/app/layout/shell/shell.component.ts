// T054/T071: Shell component – two-column layout (sidebar + chat window)
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { ChannelsListComponent } from '../../channels/channels-list.component';
import { ChatWindowComponent } from '../../chat/chat-window/chat-window.component';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [CommonModule, ChannelsListComponent, ChatWindowComponent],
  templateUrl: './shell.component.html',
  styleUrl: './shell.component.css'
})
export class ShellComponent {}
