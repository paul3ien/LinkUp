// T040 - NotificationService (gRPC ChatService) Unit Tests
using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using LinkUp.Chat;
using Microsoft.Extensions.Logging;
using Moq;
using NotificationService.Services;
using Xunit;

namespace LinkUp.Tests.NotificationService.Services;

public class NotificationServiceTests
{
    private readonly ChatGrpcService _service;
    private readonly Mock<IConnectionManager> _connectionManagerMock;

    public NotificationServiceTests()
    {
        _connectionManagerMock = new Mock<IConnectionManager>();
        var loggerMock = new Mock<ILogger<ChatGrpcService>>();
        _service = new ChatGrpcService(_connectionManagerMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task BroadcastMessage_WithValidRequest_ReturnsEmpty()
    {
        // T042: BroadcastMessage forwards to ConnectionManager
        var channelId = Guid.NewGuid();
        var request = new Message
        {
            Id = "msg-1",
            ChannelId = channelId.ToString(),
            UserId = "user-1",
            Content = "Hello"
        };
        
        _connectionManagerMock
            .Setup(m => m.BroadcastAsync(channelId, request))
            .Returns(Task.CompletedTask);
        
        dynamic contextMock = new Moq.Mock<ServerCallContext>(Moq.MockBehavior.Loose);
        
        var result = await _service.BroadcastMessage(request, (ServerCallContext)contextMock.Object);

        Assert.NotNull(result);
        _connectionManagerMock.Verify(m => m.BroadcastAsync(channelId, request), Times.Once);
    }

    [Fact]
    public async Task BroadcastMessage_WithValidGuid_CallsBroadcastAsync()
    {
        // T042: BroadcastMessage with valid Guid channel ID
        var channelId = Guid.NewGuid();
        var request = new Message
        {
            ChannelId = channelId.ToString(),
            Content = "test"
        };
        
        _connectionManagerMock
            .Setup(m => m.BroadcastAsync(channelId, request))
            .Returns(Task.CompletedTask);
        
        dynamic contextMock = new Moq.Mock<ServerCallContext>(Moq.MockBehavior.Loose);
        
        var result = await _service.BroadcastMessage(request, (ServerCallContext)contextMock.Object);

        Assert.NotNull(result);
        _connectionManagerMock.Verify(m => m.BroadcastAsync(channelId, request), Times.Once);
    }

    [Fact]
    public async Task BroadcastMessage_WithEmptyGuid_Succeeds()
    {
        // T042: BroadcastMessage with empty Guid (edge case)
        var request = new Message { ChannelId = Guid.Empty.ToString() };
        
        _connectionManagerMock
            .Setup(m => m.BroadcastAsync(Guid.Empty, request))
            .Returns(Task.CompletedTask);
        
        dynamic contextMock = new Moq.Mock<ServerCallContext>(Moq.MockBehavior.Loose);
        
        var result = await _service.BroadcastMessage(request, (ServerCallContext)contextMock.Object);

        Assert.NotNull(result);
    }
}
