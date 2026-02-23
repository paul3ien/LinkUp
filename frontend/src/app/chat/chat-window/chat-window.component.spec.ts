// T055/T071: Tests for ChatWindowComponent with new ChannelService/MessageService
import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { ChatWindowComponent } from './chat-window.component';
import { ChannelService, Channel } from '../../services/channel.service';
import { MessageService, Message } from '../../services/message.service';
import { AuthService } from '../../auth/auth.service';
import { BehaviorSubject, of } from 'rxjs';

const makeChannel = (id: string, name: string): Channel => ({
  id, name, description: 'Test channel'
});

const makeMessage = (id: string, content: string): Message => ({
  id, channelId: 'c1', userId: 'u1', content,
  createdAt: '2026-02-23T00:00:00Z'
});

describe('ChatWindowComponent', () => {
  let component: ChatWindowComponent;
  let fixture: ComponentFixture<ChatWindowComponent>;
  let channelService: jasmine.SpyObj<ChannelService>;
  let messageService: jasmine.SpyObj<MessageService>;
  let authService: jasmine.SpyObj<AuthService>;
  let selectedChannel$: BehaviorSubject<Channel | null>;
  let messages$: BehaviorSubject<Message[]>;

  beforeEach(async () => {
    selectedChannel$ = new BehaviorSubject<Channel | null>(null);
    messages$ = new BehaviorSubject<Message[]>([]);

    channelService = jasmine.createSpyObj('ChannelService', ['selectChannel'], {
      selectedChannel: selectedChannel$.asObservable()
    });
    messageService = jasmine.createSpyObj(
      'MessageService',
      ['getMessagesByChannelId', 'createMessage', 'clearMessages'],
      { messages: messages$.asObservable() }
    );
    authService = jasmine.createSpyObj('AuthService', ['logout']);

    await TestBed.configureTestingModule({
      imports: [ChatWindowComponent],
      providers: [
        { provide: ChannelService, useValue: channelService },
        { provide: MessageService, useValue: messageService },
        { provide: AuthService, useValue: authService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ChatWindowComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load messages when channel is selected', fakeAsync(() => {
    const channel = makeChannel('c1', 'General');
    selectedChannel$.next(channel);
    tick();
    expect(messageService.getMessagesByChannelId).toHaveBeenCalledWith('c1');
  }));

  it('should clear messages when no channel is selected', fakeAsync(() => {
    selectedChannel$.next(null);
    tick();
    expect(messageService.clearMessages).toHaveBeenCalled();
  }));

  it('should display channel name in header when channel is selected', fakeAsync(() => {
    const channel = makeChannel('c1', 'General');
    selectedChannel$.next(channel);
    tick();
    fixture.detectChanges();
    const header = fixture.nativeElement.querySelector('header span');
    expect(header.textContent).toContain('General');
  }));

  it('should not send when draft is empty', () => {
    component.draft = '';
    component.send();
    expect(messageService.createMessage).not.toHaveBeenCalled();
  });

  it('should not send when no channel is selected', () => {
    selectedChannel$.next(null);
    component.draft = 'hello';
    component.send();
    expect(messageService.createMessage).not.toHaveBeenCalled();
  });

  it('should send message when channel is selected and draft is not empty', () => {
    const channel = makeChannel('c1', 'General');
    selectedChannel$.next(channel);
    messageService.getMessagesByChannelId.and.returnValue(of([]));
    component.draft = 'Hello world';
    component.send();
    expect(messageService.createMessage).toHaveBeenCalledWith('c1', { content: 'Hello world' });
  });

  it('should clear draft after sending', () => {
    const channel = makeChannel('c1', 'General');
    selectedChannel$.next(channel);
    messageService.createMessage.and.returnValue(of({} as Message));
    component.draft = 'test message';
    component.send();
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

  it('should display messages from messageService', fakeAsync(() => {
    const msgs = [makeMessage('m1', 'Hello!')];
    messages$.next(msgs);
    tick();
    fixture.detectChanges();
    const messageElements = fixture.nativeElement.querySelectorAll('[class*="flex flex-col"]');
    // We should see the message in the template
    expect(messageElements.length).toBeGreaterThan(0);
  }));
});
