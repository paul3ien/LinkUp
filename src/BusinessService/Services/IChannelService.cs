// T030 - Channel Service Interface
namespace BusinessService.Services;

using BusinessService.Models;

public interface IChannelService
{
    /// <summary>
    /// T030: Get all channels (paginated)
    /// </summary>
    Task<List<Channel>> GetChannelsAsync(int page = 1, int pageSize = 10);

    /// <summary>
    /// T030: Get channel by ID
    /// </summary>
    Task<Channel?> GetChannelByIdAsync(Guid channelId);

    /// <summary>
    /// T030: Create new channel
    /// </summary>
    Task<Channel> CreateChannelAsync(string name, string createdBy);

    /// <summary>
    /// T030: Update channel
    /// </summary>
    Task<Channel> UpdateChannelAsync(Guid channelId, string name);

    /// <summary>
    /// T030: Delete channel
    /// </summary>
    Task<bool> DeleteChannelAsync(Guid channelId);
}
