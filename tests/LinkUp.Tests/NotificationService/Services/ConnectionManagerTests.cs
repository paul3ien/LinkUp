// T041 - ConnectionManager Unit Tests
using System;
using System.Threading.Tasks;
using Grpc.Core;
using LinkUp.Chat;
using Microsoft.Extensions.Logging;
using Moq;
using NotificationService.Services;
using Xunit;

namespace LinkUp.Tests.NotificationService.Services;

public class ConnectionManagerTests
{
    private readonly ConnectionManager _manager;
    private readonly Mock<ILogger<ConnectionManager>> _loggerMock;

    public ConnectionManagerTests()
    {
        _loggerMock = new Mock<ILogger<ConnectionManager>>();
        _manager = new ConnectionManager(_loggerMock.Object);
    }

    [Fact]
    public async Task RegisterSubscriberAsync_WithValidData_RegistersSubscriber()
    {
        // T041: Register a subscriber
        var channelId = Guid.NewGuid();
        var userId = "user1";
        var streamMock = new Mock<IServerStreamWriter<Message>>();

        await _manager.RegisterSubscriberAsync(channelId, userId, streamMock.Object);

        int count = _manager.GetSubscriberCount(channelId);
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task RegisterSubscriberAsync_MultipleUsers_CountsCorrectly()
    {
        // T041: Multiple subscribers on same channel
        var channelId = Guid.NewGuid();
        var streamMock1 = new Mock<IServerStreamWriter<Message>>();
        var streamMock2 = new Mock<IServerStreamWriter<Message>>();

        await _manager.RegisterSubscriberAsync(channelId, "user1", streamMock1.Object);
        await _manager.RegisterSubscriberAsync(channelId, "user2", streamMock2.Object);

        int count = _manager.GetSubscriberCount(channelId);
        Assert.Equal(2, count);
    }

    [Fact]
    public async Task UnregisterSubscriberAsync_RemovesSubscriber()
    {
        // T041: Register then unregister
        var channelId = Guid.NewGuid();
        var userId = "user1";
        var streamMock = new Mock<IServerStreamWriter<Message>>();

        await _manager.RegisterSubscriberAsync(channelId, userId, streamMock.Object);
        Assert.Equal(1, _manager.GetSubscriberCount(channelId));

        await _manager.UnregisterSubscriberAsync(channelId, userId);
        Assert.Equal(0, _manager.GetSubscriberCount(channelId));
    }

    [Fact]
    public void GetSubscriberCount_WithNoSubscribers_ReturnsZero()
    {
        // T041: Unregistered channel has no subscribers
        var channelId = Guid.NewGuid();

        int count = _manager.GetSubscriberCount(channelId);

        Assert.Equal(0, count);
    }

    [Fact]
    public async Task BroadcastAsync_SendsMessageToSubscriber()
    {
        // T042: Broadcast to single subscriber
        var channelId = Guid.NewGuid();
        var streamMock = new Mock<IServerStreamWriter<Message>>();
        var message = new Message { Id = "msg1", Content = "Hello" };

        await _manager.RegisterSubscriberAsync(channelId, "user1", streamMock.Object);
        await _manager.BroadcastAsync(channelId, message);

        streamMock.Verify(
            s => s.WriteAsync(It.Is<Message>(m => m.Id == "msg1")),
            Times.Once);
    }

    [Fact]
    public async Task BroadcastAsync_WithNoSubscribers_DoesNotThrow()
    {
        // T042: Broadcasting to empty channel is safe
        var channelId = Guid.NewGuid();
        var message = new Message { Content = "test" };

        var ex = await Record.ExceptionAsync(
            () => _manager.BroadcastAsync(channelId, message));

        Assert.Null(ex);
    }

    [Fact]
    public async Task RegisterSubscriberAsync_DuplicateUser_ReplacesPreviousStream()
    {
        // T041: Same user reconnects - replaces stream writer
        var channelId = Guid.NewGuid();
        var userId = "user1";
        var streamMock1 = new Mock<IServerStreamWriter<Message>>();
        var streamMock2 = new Mock<IServerStreamWriter<Message>>();

        await _manager.RegisterSubscriberAsync(channelId, userId, streamMock1.Object);
        await _manager.RegisterSubscriberAsync(channelId, userId, streamMock2.Object);

        // Count should still be 1 (not 2)
        int count = _manager.GetSubscriberCount(channelId);
        Assert.Equal(1, count);
    }
}
