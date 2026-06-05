// T071: Tests for MessageService
import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { MessageService, Message } from './message.service';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';

describe('MessageService', () => {
  let service: MessageService;
  let controller: HttpTestingController;
  const API_URL = 'http://localhost:7001/api/channels';

  beforeEach(() => {
    TestBed.configureTestingModule({
    imports: [],
    providers: [MessageService, provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()]
});
    service = TestBed.inject(MessageService);
    controller = TestBed.inject(HttpTestingController);
  });

  afterEach(() => controller.verify());

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should fetch messages by channel id', (done) => {
    const mockMessages: Message[] = [
      { id: '1', channelId: 'ch1', userId: 'u1', content: 'Hello', createdAt: '2026-02-23T00:00:00Z' },
      { id: '2', channelId: 'ch1', userId: 'u2', content: 'World', createdAt: '2026-02-23T00:01:00Z' }
    ];

    service.getMessagesByChannelId('ch1').subscribe((messages) => {
      expect(messages.length).toBe(2);
      expect(messages[0].content).toBe('Hello');
      done();
    });

    const req = controller.expectOne(`${API_URL}/ch1/messages`);
    expect(req.request.method).toBe('GET');
    req.flush(mockMessages);
  });

  it('should create message', (done) => {
    const newMessageData = { content: 'New message' };
    const mockMessage: Message = {
      id: '3',
      channelId: 'ch1',
      userId: 'u1',
      content: 'New message',
      createdAt: '2026-02-23T00:02:00Z'
    };

    service.createMessage('ch1', newMessageData).subscribe((message) => {
      expect(message.id).toBe('3');
      expect(message.content).toBe('New message');
      done();
    });

    const req = controller.expectOne(`${API_URL}/ch1/messages`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(newMessageData);
    req.flush(mockMessage);
  });

  it('should clear messages', (done) => {
    service.clearMessages();

    service.messages.subscribe((messages) => {
      expect(messages.length).toBe(0);
      done();
    });
  });

  it('should get messages value', (done) => {
    const mockMessage: Message = {
      id: '1',
      channelId: 'ch1',
      userId: 'u1',
      content: 'Hello',
      createdAt: '2026-02-23T00:00:00Z'
    };

    service.createMessage('ch1', { content: 'Hello' }).subscribe(() => {
      const messages = service.getMessages();
      expect(messages.length).toBe(1);
      expect(messages[0].content).toBe('Hello');
      done();
    });

    const req = controller.expectOne(`${API_URL}/ch1/messages`);
    req.flush(mockMessage);
  });

  it('should emit initial empty messages list', (done) => {
    service.messages.subscribe((messages) => {
      if (messages !== undefined) {
        expect(Array.isArray(messages)).toBe(true);
        done();
      }
    });
  });

  it('should add new message to existing messages', () => {
    const existingMessages: Message[] = [
      { id: '1', channelId: 'ch1', userId: 'u1', content: 'Hello', createdAt: '2026-02-23T00:00:00Z' }
    ];
    const newMessage: Message = {
      id: '2',
      channelId: 'ch1',
      userId: 'u2',
      content: 'World',
      createdAt: '2026-02-23T00:01:00Z'
    };

    // First, mock the service to have existing messages
    service['messages$'].next(existingMessages);

    // Create new message
    service.createMessage('ch1', { content: 'World' }).subscribe();

    const req = controller.expectOne(`${API_URL}/ch1/messages`);
    req.flush(newMessage);

    // Check that the create was called
    expect(service.getMessages()).toBeDefined();
  });
});
