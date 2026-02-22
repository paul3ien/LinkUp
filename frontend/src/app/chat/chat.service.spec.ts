// T055: Tests for ChatService
import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { ChatService } from './chat.service';
import { AuthService } from '../auth/auth.service';
import type { Message } from '../../generated/chat';

const makeMessage = (id: string, content: string): Message => ({
  id, channelId: 'c1', userId: 'u1', content, createdAt: undefined
});

/** Creates an async iterable from a list of messages (simulates gRPC server stream) */
async function* makeStream(messages: Message[]): AsyncIterable<Message> {
  for (const m of messages) yield m;
}

describe('ChatService', () => {
  let service: ChatService;
  let authService: jasmine.SpyObj<AuthService>;

  beforeEach(() => {
    authService = jasmine.createSpyObj('AuthService', ['getUserId']);
    authService.getUserId.and.returnValue('user-1');

    TestBed.configureTestingModule({
      providers: [
        ChatService,
        { provide: AuthService, useValue: authService }
      ]
    });
    service = TestBed.inject(ChatService);
  });

  afterEach(() => service.leaveChannel());

  // --- initial state ---
  it('should start with empty messages$', (done) => {
    service.messages$.subscribe(msgs => {
      expect(msgs).toEqual([]);
      done();
    });
  });

  it('should start with currentChannelId === null', () => {
    expect(service.currentChannelId).toBeNull();
  });

  // --- leaveChannel ---
  it('should set currentChannelId to null on leaveChannel()', () => {
    (service as any).currentChannelId = 'ch-42';
    service.leaveChannel();
    expect(service.currentChannelId).toBeNull();
  });

  it('should abort the stream controller on leaveChannel()', () => {
    const abort = jasmine.createSpyObj<AbortController>('AbortController', ['abort']);
    (service as any).abortController = abort;
    service.leaveChannel();
    expect(abort.abort).toHaveBeenCalled();
  });

  it('should set abortController to null after leaveChannel()', () => {
    const abort = new AbortController();
    (service as any).abortController = abort;
    service.leaveChannel();
    expect((service as any).abortController).toBeNull();
  });

  // --- joinChannel ---
  it('should set currentChannelId when joining a channel', async () => {
    // Mock the gRPC client's subscribe to return an empty stream
    (service as any).client = {
      subscribe: () => ({ responses: makeStream([]) })
    };
    await service.joinChannel('chan-abc');
    // currentChannelId is set by joinChannel and only cleared by leaveChannel()
    expect(service.currentChannelId).toBe('chan-abc');
  });

  it('should reset messages to [] when joining a new channel', async () => {
    // Pre-populate with stale messages
    (service as any)._messages$.next([makeMessage('old', 'old msg')]);

    (service as any).client = {
      subscribe: () => ({ responses: makeStream([]) })
    };

    const promise = service.joinChannel('new-channel');
    // messages reset synchronously before async stream starts
    let snapshot: Message[] = [];
    service.messages$.subscribe(m => snapshot = m).unsubscribe();
    expect(snapshot).toEqual([]);
    await promise;
  });

  it('should append messages from the stream to messages$', async () => {
    const incoming = [makeMessage('m1', 'Hello'), makeMessage('m2', 'World')];
    (service as any).client = {
      subscribe: () => ({ responses: makeStream(incoming) })
    };

    await service.joinChannel('ch-1');

    // After stream finishes, messages should contain all received items
    let final: Message[] = [];
    service.messages$.subscribe(m => final = m).unsubscribe();
    expect(final.length).toBe(2);
    expect(final[0].content).toBe('Hello');
    expect(final[1].content).toBe('World');
  });

  it('should call leaveChannel() on ngOnDestroy()', () => {
    const spy = spyOn(service, 'leaveChannel');
    service.ngOnDestroy();
    expect(spy).toHaveBeenCalled();
  });
});
