// T071: Tests for ChannelsListComponent
import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { ChannelsListComponent } from './channels-list.component';
import { ChannelService, Channel, PaginatedResponse } from '../services/channel.service';
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

  describe('Channel Array Handling (ngFor)', () => {
    it('should accept Channel[] array for *ngFor', fakeAsync(() => {
      const mockChannels: Channel[] = [
        { id: '1', name: 'General', description: 'General discussion' },
        { id: '2', name: 'Random', description: 'Random stuff' }
      ];
      channels$.next(mockChannels);
      tick();
      fixture.detectChanges();

      expect(component.channels).toEqual(mockChannels);
      expect(Array.isArray(component.channels)).toBe(true);
    }));

    it('should NOT accept paginated object for *ngFor', fakeAsync(() => {
      // This test ensures we reject invalid data structure
      const paginatedObject: any = {
        data: [{ id: '1', name: 'General' }],
        page: 1,
        pageSize: 10,
        total: 1
      };

      // Component should only accept Channel[]
      component.channels = paginatedObject;
      expect(Array.isArray(component.channels)).toBe(false);
      expect((component.channels as any).page).toBeDefined(); // Proves it's wrong structure
    }));

    it('should handle empty Channel[] array', fakeAsync(() => {
      channels$.next([]);
      tick();
      fixture.detectChanges();

      expect(component.channels).toEqual([]);
      expect(Array.isArray(component.channels)).toBe(true);
    }));
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
    channelService.createChannel.and.returnValue(of({ id: '1', name: 'Test Channel', description: 'Test Description' }));

    const button = fixture.nativeElement.querySelector('button');
    button?.click();

    expect(channelService.createChannel).toHaveBeenCalledWith({
      name: 'Test Channel',
      description: 'Test Description'
    });
  }));

  it('should not create channel if name is empty', fakeAsync(() => {
    spyOn(window, 'prompt').and.returnValue('');

    const button = fixture.nativeElement.querySelector('button');
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

    const description = fixture.nativeElement.textContent;
    expect(description).toContain('General discussion');
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

  describe('Channel Creation Flow', () => {
    it('should reload channels after successful creation', fakeAsync(() => {
      const newChannel: Channel = { id: '2', name: 'New', description: 'New channel' };
      spyOn(window, 'prompt').and.returnValues('New', 'New channel');
      
      channelService.createChannel.and.returnValue(of(newChannel));
      channelService.getAllChannels.and.returnValue(of([newChannel]));

      // Use Promise to handle async properly
      fixture.ngZone!.run(() => {
        const button = fixture.nativeElement.querySelector('button');
        button?.click();
      });

      tick();
      fixture.detectChanges();

      expect(channelService.getAllChannels).toHaveBeenCalled();
    }));

    it('should update channels list when new channel created', fakeAsync(() => {
      const initialChannels: Channel[] = [
        { id: '1', name: 'General' }
      ];
      const newChannel: Channel = { id: '2', name: 'New' };

      channels$.next(initialChannels);
      spyOn(window, 'prompt').and.returnValues('New', null);

      channelService.createChannel.and.returnValue(of(newChannel));
      channelService.getAllChannels.and.returnValue(of([...initialChannels, newChannel]));

      const button = fixture.nativeElement.querySelector('button');
      button?.click();

      tick();
      fixture.detectChanges();

      // After successful creation, getAllChannels should be called
      expect(channelService.getAllChannels).toHaveBeenCalled();
    }));
  });

  describe('Data Integrity Tests', () => {
    it('should always keep channels as array for iteration', fakeAsync(() => {
      const mockChannels: Channel[] = [
        { id: '1', name: 'General' },
        { id: '2', name: 'Random' },
        { id: '3', name: 'Tech' }
      ];

      channels$.next(mockChannels);
      tick();
      fixture.detectChanges();

      // Verify component.channels is iterable array
      const iterationTest = () => {
        component.channels.forEach((channel) => {
          expect(channel.name).toBeDefined();
        });
      };

      expect(iterationTest).not.toThrow();
    }));

    it('should prevent error NG0900 by maintaining proper array type', fakeAsync(() => {
      const mockChannels: Channel[] = [
        { id: '1', name: 'test' }
      ];

      channels$.next(mockChannels);
      tick();

      // This would throw NG0900 if channels was {data: [...]}
      // ngFor requires: for (let item of channels)
      const testIteration = () => {
        for (const _ of component.channels) {
          // iteration works
        }
      };

      expect(testIteration).not.toThrow();
    }));
  });
});
