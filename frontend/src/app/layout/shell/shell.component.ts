// T054/T071: Shell component – two-column layout (channels sidebar + router outlet)
import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ChannelsListComponent } from '../../channels/channels-list.component';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [RouterOutlet, ChannelsListComponent],
  templateUrl: './shell.component.html',
  styleUrl: './shell.component.css'
})
export class ShellComponent {}

