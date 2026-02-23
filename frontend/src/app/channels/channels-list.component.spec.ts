// T071: Tests for ChannelsListComponent
import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { ChannelsListComponent } from './channels-list.component';
import { ChannelService, Channel } from '../services/channel.service';
import { BehaviorSubject, of } from 'rxjs';

describe('ChannelsListComponent', () => {
  let component: ChannelsListComponent;
  let fixture: ComponentFixture<ChannelsListComponent>;
  let channelService: jasmine.SpyObj<ChannelService>;
  let channels$: BehaviorSubject<Channel[]>;
  let selectedChannel$: BehaviorSubject<Channel | null>;

  beforeEach(async () => {
    channels$ = new BehaviorSubject<Channel[]>([]);
    selectedChannel$ = new BehaviorSubject<Channel | null>(null);

    channelService = jasmine.createSpyObj(
      'ChannelService',
      ['getAllChannels', 'createChannel', 'selectChannel'],
      {
        channels: channels$.asObservable(),
        selectedChannel: selectedChannel$.asObservable()
      }
    );
    channelService.getAllChannels.and.returnValue(of([]));

    await TestBed.configureTestingModule({
      imports: [ChannelsListComponent],
      providers: [{ provide: ChannelService, useValue: channelService }]
    }).compileComponents();

    fixture = TestBed.createComponent(ChannelsListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load channels on init', fakeAsync(() => {
    const mockChannels: Channel[] = [
      { id: '1', name: 'General', description: 'General discussion' }
    ];
    channels$.next(mockChannels);
    tick();
    fixture.detectChanges();

    expect(component.channels).toEqual(mockChannels);
  }));

  it('should display "Aucun channel disponible" when no channels', fakeAsync(() => {
    channels$.next([]);
    tick();
    fixture.detectChanges();

    const emptyMessage = fixture.nativeElement.querySelector('[class*="text-gray-400"]');
    expect(emptyMessage?.textContent).toContain('Aucun channel');
  }));

  it('should display channels list', fakeAsync(() => {
    const mockChannels: Channel[] = [
      { id: '1', name: 'General', description: 'General discussion' },
      { id: '2', name: 'Random', description: 'Random stuff' }
    ];
    channels$.next(mockChannels);
    tick();
    fixture.detectChanges();

    const channelDivs = fixture.nativeElement.querySelectorAll('[class*="cursor-pointer"]');
    expect(channelDivs.length).toBe(2);
  }));

  it('should select channel on click', fakeAsync(() => {
    const channel: Channel = { id: '1', name: 'General', description: 'General discussion' };
    channels$.next([channel]);
    tick();
    fixture.detectChanges();

    const channelDiv = fixture.nativeElement.querySelector('[class*="cursor-pointer"]');
    channelDiv?.click();

    expect(channelService.selectChannel).toHaveBeenCalledWith(channel);
  }));

  it('should highlight selected channel', fakeAsync(() => {
    const channel: Channel = { id: '1', name: 'General', description: 'General discussion' };
    channels$.next([channel]);
    selectedChannel$.next(channel);
    tick();
    fixture.detectChanges();

    const channelDiv = fixture.nativeElement.querySelector('[class*="cursor-pointer"]');
    expect(channelDiv?.classList.contains('bg-indigo-600')).toBe(true);
  }));

  it('should create new channel on button click', fakeAsync(() => {
    spyOn(window, 'prompt').and.returnValues('Test Channel', 'Test Description');
    channelService.createChannel.and.returnValue(of({} as Channel));

    const button = fixture.nativeElement.querySelector('[class*="bg-indigo-600"]');
    button?.click();

    expect(channelService.createChannel).toHaveBeenCalledWith({
      name: 'Test Channel',
      description: 'Test Description'
    });
  }));

  it('should not create channel if name is empty', fakeAsync(() => {
    spyOn(window, 'prompt').and.returnValue('');

    const button = fixture.nativeElement.querySelector('[class*="bg-indigo-600"]');
    button?.click();

    expect(channelService.createChannel).not.toHaveBeenCalled();
  }));

  it('should display channel description', fakeAsync(() => {
    const mockChannels: Channel[] = [
      { id: '1', name: 'General', description: 'General discussion' }
    ];
    channels$.next(mockChannels);
    tick();
    fixture.detectChanges();

    const description = fixture.nativeElement.querySelector('[class*="text-xs text-gray-400 mt-1"]');
    expect(description?.textContent).toContain('General discussion');
  }));

  it('should check if channel is selected', () => {
    const channel: Channel = { id: '1', name: 'General' };
    component.selectedChannel = channel;

    expect(component.isSelected(channel)).toBe(true);
  });

  it('should return false for unselected channel', () => {
    const channel1: Channel = { id: '1', name: 'General' };
    const channel2: Channel = { id: '2', name: 'Random' };
    component.selectedChannel = channel1;

    expect(component.isSelected(channel2)).toBe(false);
  });
});
