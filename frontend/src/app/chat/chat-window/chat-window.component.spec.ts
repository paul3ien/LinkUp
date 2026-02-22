// T055: Tests for ChatWindowComponent
import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { ChatWindowComponent } from './chat-window.component';
import { ChatService } from '../chat.service';
import { AuthService } from '../../auth/auth.service';
import { BehaviorSubject } from 'rxjs';
import type { Message } from '../../../generated/chat';

const makeMessage = (id: string, content: string): Message => ({
  id, channelId: 'c1', userId: 'u1', content,
  createdAt: undefined
});

describe('ChatWindowComponent', () => {
  let component: ChatWindowComponent;
  let fixture: ComponentFixture<ChatWindowComponent>;
  let controller: HttpTestingController;
  let chatService: jasmine.SpyObj<ChatService>;
  let authService: jasmine.SpyObj<AuthService>;
  let messages$: BehaviorSubject<Message[]>;

  beforeEach(async () => {
    messages$ = new BehaviorSubject<Message[]>([]);
    chatService = jasmine.createSpyObj('ChatService', ['joinChannel', 'leaveChannel'], {
      currentChannelId: 'chan-1',
      messages$: messages$.asObservable()
    });
    authService = jasmine.createSpyObj('AuthService', ['logout']);

    await TestBed.configureTestingModule({
      imports: [ChatWindowComponent, HttpClientTestingModule],
      providers: [
        { provide: ChatService, useValue: chatService },
        { provide: AuthService, useValue: authService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ChatWindowComponent);
    component = fixture.componentInstance;
    controller = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
  });

  afterEach(() => controller.verify());

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should not send when draft is empty', () => {
    component.draft = '';
    component.send();
    controller.expectNone((req) => true);
  });

  it('should not send when no channel is selected', () => {
    Object.defineProperty(chatService, 'currentChannelId', { get: () => null, configurable: true });
    component.draft = 'hello';
    component.send();
    controller.expectNone((req) => true);
  });

  it('should POST message content to the correct channel endpoint', () => {
    component.draft = 'Hello world';
    component.send();
    const req = controller.expectOne('http://localhost:5002/api/channels/chan-1/messages');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ content: 'Hello world' });
    req.flush({});
  });

  it('should clear draft after sending', () => {
    component.draft = 'test message';
    component.send();
    controller.expectOne('http://localhost:5002/api/channels/chan-1/messages').flush({});
    expect(component.draft).toBe('');
  });

  it('should call send() on Enter keydown', () => {
    const sendSpy = spyOn(component, 'send');
    const event = new KeyboardEvent('keydown', { key: 'Enter' });
    component.onKeydown(event);
    expect(sendSpy).toHaveBeenCalled();
  });

  it('should NOT call send() on Shift+Enter', () => {
    const sendSpy = spyOn(component, 'send');
    const event = new KeyboardEvent('keydown', { key: 'Enter', shiftKey: true });
    component.onKeydown(event);
    expect(sendSpy).not.toHaveBeenCalled();
  });

  it('should call AuthService.logout() on logout()', () => {
    component.logout();
    expect(authService.logout).toHaveBeenCalled();
  });

  it('should expose messages$ from ChatService', fakeAsync(() => {
    const msgs = [makeMessage('m1', 'Hello!')];
    messages$.next(msgs);
    tick();
    let received: Message[] = [];
    component.messages$.subscribe(m => received = m);
    expect(received).toEqual(msgs);
  }));
});
