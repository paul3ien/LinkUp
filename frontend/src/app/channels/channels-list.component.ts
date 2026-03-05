// T071: Channels List Component - Sidebar with channel list
import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ChannelService, Channel } from '../services/channel.service';
import { Subject, takeUntil } from 'rxjs';

@Component({
  selector: 'app-channels-list',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="h-full flex flex-col bg-gray-900 text-white">
      <!-- Header -->
      <div class="p-4 border-b border-gray-700">
        <h2 class="text-lg font-bold">Channels</h2>
      </div>

      <!-- Channels List -->
      <div class="flex-1 overflow-y-auto">
        <div *ngIf="channels.length === 0" class="p-4 text-gray-400 text-sm">
          Aucun channel disponible
        </div>
        
        <div *ngFor="let channel of channels"
          (click)="selectChannel(channel)"
          [class.bg-indigo-600]="isSelected(channel)"
          class="p-4 cursor-pointer hover:bg-gray-800 transition border-b border-gray-700">
          <div class="font-semibold"># {{ channel.name }}</div>
          <div *ngIf="channel.description" class="text-xs text-gray-400 mt-1">
            {{ channel.description }}
          </div>
        </div>
      </div>

      <!-- New Channel Button -->
      <div class="p-4 border-t border-gray-700">
        <button (click)="createNewChannel()"
          class="w-full bg-indigo-600 hover:bg-indigo-700 text-white font-semibold py-2 rounded-lg transition">
          + Nouveau channel
        </button>
      </div>
    </div>
  `,
  styles: [`
    :host {
      display: block;
      height: 100%;
    }
  `]
})
export class ChannelsListComponent implements OnInit, OnDestroy {
  private channelService = inject(ChannelService);
  private destroy$ = new Subject<void>();

  channels: Channel[] = [];
  selectedChannel: Channel | null = null;

  ngOnInit(): void {
    // Load channels on init
    this.loadChannels();

    // Subscribe to channels updates
    this.channelService.channels.pipe(
      takeUntil(this.destroy$)
    ).subscribe((channels: Channel[]) => {
      console.log('Canaux reçus:', channels);
      this.channels = channels;
    });

    // Subscribe to selected channel
    this.channelService.selectedChannel.pipe(
      takeUntil(this.destroy$)
    ).subscribe((channel: Channel | null) => {
      this.selectedChannel = channel;
    });
  }

  private loadChannels(): void {
    this.channelService.getAllChannels().pipe(
      takeUntil(this.destroy$)
    ).subscribe({
      next: (channels) => {
        console.log('Canaux chargés:', channels);
      },
      error: (err) => {
        console.error('Erreur lors du chargement des canaux:', err);
      }
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  selectChannel(channel: Channel): void {
    this.channelService.selectChannel(channel);
  }

  isSelected(channel: Channel): boolean {
    return this.selectedChannel?.id === channel.id;
  }

  createNewChannel(): void {
    const name = prompt('Nom du channel:');
    if (!name) return;
    
    const description = prompt('Description (optionnelle):');
    console.log('🔄 Création du canal:', { name, description });
    
    this.channelService.createChannel({ name, description: description || undefined }).subscribe({
      next: (channel) => {
        console.log('✅ Canal créé avec succès:', channel);
        // Recharger la liste des channels après création
        this.channelService.getAllChannels().subscribe();
      },
      error: (err: any) => {
        console.error('❌ Erreur création canal:', err);
        alert('Erreur: ' + (err.error?.message || 'Erreur inconnue'));
      }
    });
  }
}
