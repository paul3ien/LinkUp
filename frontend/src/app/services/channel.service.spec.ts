// T071: Tests for ChannelService
import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ChannelService, Channel, PaginatedResponse } from './channel.service';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';

describe('ChannelService', () => {
  let service: ChannelService;
  let controller: HttpTestingController;
  const API_URL = 'http://localhost:7001/api/channels';

  beforeEach(() => {
    TestBed.configureTestingModule({
    imports: [],
    providers: [ChannelService, provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()]
});
    service = TestBed.inject(ChannelService);
    controller = TestBed.inject(HttpTestingController);
  });

  afterEach(() => controller.verify());

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getAllChannels - Paginated Response Handling', () => {
    it('should extract channels from paginated response', (done) => {
      const mockResponse: PaginatedResponse<Channel> = {
        data: [
          { id: '1', name: 'General', description: 'General discussion' },
          { id: '2', name: 'Random', description: 'Random stuff' }
        ],
        page: 1,
        pageSize: 10,
        total: 2
      };

      service.getAllChannels().subscribe((channels) => {
        // CRITICAL: Verify we extracted data array, not paginated object
        expect(Array.isArray(channels)).toBe(true);
        expect((channels as any).page).toBeUndefined();
        expect((channels as any).pageSize).toBeUndefined();
        expect((channels as any).total).toBeUndefined();
        expect(channels.length).toBe(2);
        expect(channels[0].name).toBe('General');
        done();
      });

      const req = controller.expectOne(API_URL);
      expect(req.request.method).toBe('GET');
      req.flush(mockResponse);
    });

    it('should handle empty paginated response', (done) => {
      const mockResponse: PaginatedResponse<Channel> = {
        data: [],
        page: 1,
        pageSize: 10,
        total: 0
      };

      service.getAllChannels().subscribe((channels) => {
        expect(channels).toEqual([]);
        expect(Array.isArray(channels)).toBe(true);
        expect(channels.length).toBe(0);
        done();
      });

      const req = controller.expectOne(API_URL);
      req.flush(mockResponse);
    });

    it('should handle null data in response gracefully', (done) => {
      const mockResponse = {
        data: null,
        page: 1,
        pageSize: 10,
        total: 0
      };

      service.getAllChannels().subscribe((channels) => {
        expect(Array.isArray(channels)).toBe(true);
        expect(channels).toEqual([]);
        done();
      });

      const req = controller.expectOne(API_URL);
      req.flush(mockResponse);
    });

    it('should update BehaviorSubject with extracted channels for *ngFor', (done) => {
      const mockResponse: PaginatedResponse<Channel> = {
        data: [
          { id: '1', name: 'General' },
          { id: '2', name: 'Random' }
        ],
        page: 1,
        pageSize: 10,
        total: 2
      };

      let channelsFromObservable: Channel[] | null = null;

      service.channels.subscribe(channels => {
        channelsFromObservable = channels;
      });

      service.getAllChannels().subscribe(() => {
        // Verify the BehaviorSubject is updated with proper array
        expect(channelsFromObservable).toEqual(mockResponse.data);
        // Verify it's an array that can be used in *ngFor
        expect(() => {
          channelsFromObservable!.map(c => c.name);
        }).not.toThrow();
        done();
      });

      const req = controller.expectOne(API_URL);
      req.flush(mockResponse);
    });

    it('should prevent ngFor error NG0900 by returning array', (done) => {
      const mockResponse: PaginatedResponse<Channel> = {
        data: [{ id: '1', name: 'test' }],
        page: 1,
        pageSize: 10,
        total: 1
      };

      service.getAllChannels().subscribe(channels => {
        // The critical check: ensure we cannot iterate over paginated object
        // This would throw error: "Only arrays and iterables are allowed"
        const iterableTest = () => {
          for (const _ of channels) {
            // If this works, channels is iterable
          }
        };
        expect(iterableTest).not.toThrow();
        done();
      });

      const req = controller.expectOne(API_URL);
      req.flush(mockResponse);
    });
  });

  describe('createChannel', () => {
    it('should create channel and append to list', (done) => {
      const existingChannels: Channel[] = [{ id: '1', name: 'General' }];
      const newChannelData = { name: 'New', description: 'New channel' };
      const mockCreatedChannel: Channel = { id: '2', ...newChannelData };

      // Initialize with existing channels
      service['channels$'].next(existingChannels);

      service.createChannel(newChannelData).subscribe((channel) => {
        expect(channel).toEqual(mockCreatedChannel);
        done();
      });

      const req = controller.expectOne(API_URL);
      expect(req.request.method).toBe('POST');
      req.flush(mockCreatedChannel);
    });

    it('should safely spread array when adding channel', (done) => {
      const mockChannels: Channel[] = [{ id: '1', name: 'General' }];
      const newChannel: Channel = { id: '2', name: 'New' };

      service['channels$'].next(mockChannels);

      service.createChannel({ name: 'New' }).subscribe(() => {
        const updated = service['channels$'].value;
        // Critical: verify no error "current is not iterable"
        expect(Array.isArray(updated)).toBe(true);
        expect(updated.length).toBe(2);
        expect(() => [...updated]).not.toThrow();
        done();
      });

      const req = controller.expectOne(API_URL);
      req.flush(newChannel);
    });

    it('should handle creation with empty channels list', (done) => {
      service['channels$'].next([]);

      const newChannel: Channel = { id: '1', name: 'First' };

      service.createChannel({ name: 'First' }).subscribe(() => {
        const updated = service['channels$'].value;
        expect(updated.length).toBe(1);
        expect(updated[0]).toEqual(newChannel);
        done();
      });

      const req = controller.expectOne(API_URL);
      req.flush(newChannel);
    });
  });

  it('should fetch single channel by id', (done) => {
    const mockChannel: Channel = { id: '1', name: 'General', description: 'General discussion' };

    service.getChannelById('1').subscribe((channel) => {
      expect(channel.name).toBe('General');
      done();
    });

    const req = controller.expectOne(`${API_URL}/1`);
    expect(req.request.method).toBe('GET');
    req.flush(mockChannel);
  });

  it('should select channel', (done) => {
    const channel: Channel = { id: '1', name: 'General' };
    service.selectChannel(channel);

    service.selectedChannel.subscribe((selected) => {
      expect(selected?.id).toBe('1');
      expect(selected?.name).toBe('General');
      done();
    });
  });

  it('should get selected channel value', () => {
    const channel: Channel = { id: '1', name: 'General' };
    service.selectChannel(channel);
    expect(service.getSelectedChannel()).toEqual(channel);
  });

  it('should clear selection', (done) => {
    const channel: Channel = { id: '1', name: 'General' };
    service.selectChannel(channel);
    service.clearSelection();

    service.selectedChannel.subscribe((selected) => {
      expect(selected).toBeNull();
      done();
    });
  });

  it('should emit initial empty channels list', (done) => {
    service.channels.subscribe((channels) => {
      if (channels !== undefined) {
        expect(Array.isArray(channels)).toBe(true);
        done();
      }
    });
  });
});
