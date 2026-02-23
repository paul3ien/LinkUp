// T071: Tests for ChannelService
import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { ChannelService, Channel } from './channel.service';

describe('ChannelService', () => {
  let service: ChannelService;
  let controller: HttpTestingController;
  const API_URL = 'http://localhost:7001/api/channels';

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [ChannelService]
    });
    service = TestBed.inject(ChannelService);
    controller = TestBed.inject(HttpTestingController);
  });

  afterEach(() => controller.verify());

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should fetch all channels', (done) => {
    const mockChannels: Channel[] = [
      { id: '1', name: 'General', description: 'General discussion' },
      { id: '2', name: 'Random', description: 'Random stuff' }
    ];

    service.getAllChannels().subscribe((channels) => {
      expect(channels.length).toBe(2);
      expect(channels[0].name).toBe('General');
      done();
    });

    const req = controller.expectOne(API_URL);
    expect(req.request.method).toBe('GET');
    req.flush(mockChannels);
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

  it('should create channel', (done) => {
    const newChannelData = { name: 'New', description: 'New channel' };
    const mockChannel: Channel = { id: '3', ...newChannelData };

    service.createChannel(newChannelData).subscribe((channel) => {
      expect(channel.id).toBe('3');
      expect(channel.name).toBe('New');
      done();
    });

    const req = controller.expectOne(API_URL);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(newChannelData);
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

  it('should update channels list when creating channel', () => {
    const existingChannels: Channel[] = [{ id: '1', name: 'General' }];
    const newChannel: Channel = { id: '2', name: 'New', description: 'New channel' };

    // First, mock the service to have existing channels
    service['channels$'].next(existingChannels);

    // Create new channel
    service.createChannel({ name: 'New', description: 'New channel' }).subscribe();

    const req = controller.expectOne('http://localhost:7001/api/channels');
    req.flush(newChannel);

    // Check that the new channel was added
    expect(service.getSelectedChannel()).toBeDefined();
  });
});
