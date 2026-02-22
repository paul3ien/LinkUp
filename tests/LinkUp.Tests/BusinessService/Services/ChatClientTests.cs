// T033 - ChatClient Unit Tests
using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using BusinessService.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace LinkUp.Tests.BusinessService.Services;

public class ChatClientTests
{
    private readonly Mock<ILogger<ChatClient>> _mockLogger;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly ChatClient _client;

    public ChatClientTests()
    {
        _mockLogger = new Mock<ILogger<ChatClient>>();
        _mockConfiguration = new Mock<IConfiguration>();
        _client = new ChatClient(_mockLogger.Object, _mockConfiguration.Object);
    }

    [Fact]
    public async Task BroadcastMessageAsync_WithValidData_ShouldReturnSuccess()
    {
        // T033: gRPC broadcast should return success

        // Arrange
        var channelId = Guid.NewGuid();
        var messageId = Guid.NewGuid();
        var userId = "user@example.com";
        var content = "Hello, everyone!";

        // Act
        var result = await _client.BroadcastMessageAsync(channelId, messageId, userId, content);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task BroadcastMessageAsync_ShouldNotThrowException()
    {
        // T033: Broadcasting should handle errors gracefully

        // Arrange
        var channelId = Guid.NewGuid();
        var messageId = Guid.NewGuid();
        var userId = "user@example.com";
        var content = "Test message";

        // Act & Assert - Should not throw
        var result = await _client.BroadcastMessageAsync(channelId, messageId, userId, content);
        Assert.True(result);
    }

    [Fact]
    public async Task BroadcastMessageAsync_WithEmptyGuids_ShouldReturnSuccess()
    {
        // T033: Should handle empty GUIDs

        // Arrange
        var channelId = Guid.Empty;
        var messageId = Guid.Empty;
        var userId = string.Empty;
        var content = string.Empty;

        // Act
        var result = await _client.BroadcastMessageAsync(channelId, messageId, userId, content);

        // Assert
        Assert.True(result);
    }
}
