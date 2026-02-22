// T055: ChatService – gRPC-Web streaming via @protobuf-ts + BehaviorSubject
import { Injectable, inject, OnDestroy } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { GrpcWebFetchTransport } from '@protobuf-ts/grpcweb-transport';
import { ChatServiceClient } from '../../generated/chat.client';
import type { Message } from '../../generated/chat';
import { AuthService } from '../auth/auth.service';

@Injectable({ providedIn: 'root' })
export class ChatService implements OnDestroy {
  private readonly auth = inject(AuthService);
  private readonly transport = new GrpcWebFetchTransport({ baseUrl: 'http://localhost:5000' });
  private readonly client = new ChatServiceClient(this.transport);

  private readonly _messages$ = new BehaviorSubject<Message[]>([]);
  readonly messages$ = this._messages$.asObservable();

  private abortController: AbortController | null = null;
  currentChannelId: string | null = null;

  async joinChannel(channelId: string): Promise<void> {
    this.leaveChannel();
    this.currentChannelId = channelId;
    this._messages$.next([]);

    const userId = this.auth.getUserId() ?? 'anonymous';
    this.abortController = new AbortController();

    const call = this.client.subscribe(
      { channelId, userId },
      { abort: this.abortController.signal }
    );

    try {
      for await (const message of call.responses) {
        this._messages$.next([...this._messages$.getValue(), message]);
      }
    } catch {
      // stream closed or aborted – expected on channel switch / logout
    }
  }

  leaveChannel(): void {
    this.abortController?.abort();
    this.abortController = null;
    this.currentChannelId = null;
  }

  ngOnDestroy(): void { this.leaveChannel(); }
}
