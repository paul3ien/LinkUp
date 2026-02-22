// T030 - Channel Service Implementation
using BusinessService.Data;
using BusinessService.Models;
using Microsoft.EntityFrameworkCore;

namespace BusinessService.Services;

public class ChannelService : IChannelService
{
    private readonly BusinessDbContext _context;
    private readonly ILogger<ChannelService> _logger;

    public ChannelService(BusinessDbContext context, ILogger<ChannelService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// T030: Get all channels with pagination
    /// </summary>
    public async Task<List<Channel>> GetChannelsAsync(int page = 1, int pageSize = 10)
    {
        try
        {
            var skip = (page - 1) * pageSize;
            var channels = await _context.Channels
                .OrderByDescending(c => c.CreatedAt)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();
            
            _logger.LogInformation("Retrieved {Count} channels (page {Page})", channels.Count, page);
            return channels;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving channels");
            throw;
        }
    }

    /// <summary>
    /// T030: Get channel by ID
    /// </summary>
    public async Task<Channel?> GetChannelByIdAsync(Guid channelId)
    {
        try
        {
            var channel = await _context.Channels
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.Id == channelId);
            
            if (channel == null)
            {
                _logger.LogWarning("Channel {ChannelId} not found", channelId);
            }
            
            return channel;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving channel {ChannelId}", channelId);
            throw;
        }
    }

    /// <summary>
    /// T030: Create new channel
    /// </summary>
    public async Task<Channel> CreateChannelAsync(string name, string createdBy)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Channel name cannot be empty");

            var channel = new Channel
            {
                Id = Guid.NewGuid(),
                Name = name,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            };

            _context.Channels.Add(channel);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Channel {ChannelId} created by {User}", channel.Id, createdBy);
            return channel;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating channel");
            throw;
        }
    }

    /// <summary>
    /// T030: Update channel
    /// </summary>
    public async Task<Channel> UpdateChannelAsync(Guid channelId, string name)
    {
        try
        {
            var channel = await _context.Channels.FirstOrDefaultAsync(c => c.Id == channelId);
            if (channel == null)
                throw new InvalidOperationException($"Channel {channelId} not found");

            channel.Name = name;
            _context.Channels.Update(channel);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Channel {ChannelId} updated", channelId);
            return channel;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating channel {ChannelId}", channelId);
            throw;
        }
    }

    /// <summary>
    /// T030: Delete channel
    /// </summary>
    public async Task<bool> DeleteChannelAsync(Guid channelId)
    {
        try
        {
            var channel = await _context.Channels.FirstOrDefaultAsync(c => c.Id == channelId);
            if (channel == null)
                return false;

            _context.Channels.Remove(channel);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Channel {ChannelId} deleted", channelId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting channel {ChannelId}", channelId);
            throw;
        }
    }
}
