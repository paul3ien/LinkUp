// T055: ChatService – gRPC-Web streaming via @protobuf-ts + BehaviorSubject
import { Injectable, inject, OnDestroy } from '@angular/core';
import { GrpcWebFetchTransport } from '@protobuf-ts/grpcweb-transport';
import { ChatServiceClient } from '../../generated/chat.client';
import { AuthService } from '../auth/auth.service';
import { MessageService } from '../services/message.service';

@Injectable({ providedIn: 'root' })
export class ChatService implements OnDestroy {
  private readonly auth = inject(AuthService);
  private readonly messageService = inject(MessageService);
  private readonly transport = new GrpcWebFetchTransport({ baseUrl: 'http://localhost:7002' });
  private readonly client = new ChatServiceClient(this.transport);

  private abortController: AbortController | null = null;
  currentChannelId: string | null = null;

  async joinChannel(channelId: string): Promise<void> {
    this.leaveChannel();
    this.currentChannelId = channelId;

    const userId = this.auth.getUserId() ?? 'anonymous';
    this.abortController = new AbortController();

    const call = this.client.subscribe(
      { channelId, userId },
      { abort: this.abortController.signal }
    );

    try {
      for await (const grpcMsg of call.responses) {
        // Convert proto Timestamp to ISO string for MessageService
        let createdAt = new Date().toISOString();
        if (grpcMsg.createdAt) {
          createdAt = new Date(Number(grpcMsg.createdAt.seconds) * 1000).toISOString();
        }
        this.messageService.addRealtimeMessage({
          id: grpcMsg.id,
          channelId: grpcMsg.channelId,
          userId: grpcMsg.userId,
          content: grpcMsg.content,
          createdAt
        });
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
