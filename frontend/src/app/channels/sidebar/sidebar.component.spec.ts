// T054: Tests for SidebarComponent
import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { SidebarComponent, Channel } from './sidebar.component';
import { ChatService } from '../../chat/chat.service';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';

describe('SidebarComponent', () => {
  let component: SidebarComponent;
  let fixture: ComponentFixture<SidebarComponent>;
  let controller: HttpTestingController;
  let chatService: jasmine.SpyObj<ChatService>;

  beforeEach(async () => {
    chatService = jasmine.createSpyObj('ChatService', ['joinChannel'], { currentChannelId: null });

    await TestBed.configureTestingModule({
    imports: [SidebarComponent],
    providers: [{ provide: ChatService, useValue: chatService }, provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()]
}).compileComponents();

    fixture = TestBed.createComponent(SidebarComponent);
    component = fixture.componentInstance;
    controller = TestBed.inject(HttpTestingController);
  });

  afterEach(() => controller.verify());

  it('should create', () => {
    fixture.detectChanges();
    controller.expectOne('http://localhost:5002/api/channels').flush([]);
    expect(component).toBeTruthy();
  });

  it('should load channels from the API on init', fakeAsync(() => {
    fixture.detectChanges();
    const mockChannels: Channel[] = [
      { id: 'c1', name: 'general', description: 'General chat' },
      { id: 'c2', name: 'random', description: 'Random' }
    ];
    controller.expectOne('http://localhost:5002/api/channels').flush(mockChannels);
    tick();
    expect(component.channels.length).toBe(2);
    expect(component.channels[0].name).toBe('general');
  }));

  it('should auto-select the first channel after load', fakeAsync(() => {
    fixture.detectChanges();
    const mockChannels: Channel[] = [{ id: 'c1', name: 'general', description: '' }];
    controller.expectOne('http://localhost:5002/api/channels').flush(mockChannels);
    tick();
    expect(chatService.joinChannel).toHaveBeenCalledWith('c1');
  }));

  it('should not call joinChannel when channel list is empty', fakeAsync(() => {
    fixture.detectChanges();
    controller.expectOne('http://localhost:5002/api/channels').flush([]);
    tick();
    expect(chatService.joinChannel).not.toHaveBeenCalled();
  }));

  it('should call joinChannel when a channel is selected', () => {
    fixture.detectChanges();
    controller.expectOne('http://localhost:5002/api/channels').flush([]);
    const ch: Channel = { id: 'c99', name: 'test', description: '' };
    component.selectChannel(ch);
    expect(chatService.joinChannel).toHaveBeenCalledWith('c99');
  });

  it('should return true from isActive() for the current channel', () => {
    Object.defineProperty(chatService, 'currentChannelId', { get: () => 'c5', configurable: true });
    fixture.detectChanges();
    controller.expectOne('http://localhost:5002/api/channels').flush([]);
    expect(component.isActive({ id: 'c5', name: 'x', description: '' })).toBeTrue();
    expect(component.isActive({ id: 'c6', name: 'y', description: '' })).toBeFalse();
  });

  it('should set channels to [] and not throw on API error', fakeAsync(() => {
    fixture.detectChanges();
    controller.expectOne('http://localhost:5002/api/channels').error(new ProgressEvent('error'));
    tick();
    expect(component.channels).toEqual([]);
  }));
});
