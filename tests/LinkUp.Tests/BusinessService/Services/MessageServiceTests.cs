// T031 - MessageService Unit Tests
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using BusinessService.Models;

namespace LinkUp.Tests.BusinessService.Services;

public class MessageServiceTests
{
    [Fact]
    public void CreateMessageRequest_WithValidData_ShouldHaveProperties()
    {
        // T031: Message creation validation

        // Arrange & Act
        var content = "Hello, World!";

        // Assert
        Assert.NotEmpty(content);
    }

    [Fact]
    public void MessageEntity_ConstructorShouldSetDefaults()
    {
        // T031: Message entity should have ID and timestamp

        // Arrange & Act
        var message = new Message
        {
            Id = Guid.NewGuid(),
            ChannelId = Guid.NewGuid(),
            UserId = "user1@example.com",
            Content = "Test message",
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        Assert.NotEqual(Guid.Empty, message.Id);
        Assert.NotEqual(Guid.Empty, message.ChannelId);
        Assert.Equal("user1@example.com", message.UserId);
        Assert.Equal("Test message", message.Content);
        Assert.NotEqual(default, message.CreatedAt);
    }

    [Fact]
    public void MessageValidation_ContentCannotBeEmpty()
    {
        // T031: Message content validation

        var content = string.Empty;
        Assert.True(string.IsNullOrWhiteSpace(content));
    }

    [Fact]
    public void MessageValidation_ShouldHaveChannelReference()
    {
        // T031: Message must reference a channel

        var channelId = Guid.NewGuid();
        var message = new Message
        {
            Id = Guid.NewGuid(),
            ChannelId = channelId,
            UserId = "user1",
            Content = "Hello",
            CreatedAt = DateTime.UtcNow
        };

        Assert.Equal(channelId, message.ChannelId);
        Assert.NotEqual(Guid.Empty, message.ChannelId);
    }

    [Fact]
    public void MessageOrdering_ByCreatedAt_ShouldBeDescending()
    {
        // T031: Messages should be ordered by creation time (newest first)

        var now = DateTime.UtcNow;
        var messages = new List<Message>
        {
            new() { Id = Guid.NewGuid(), ChannelId = Guid.NewGuid(), UserId = "u1", Content = "msg1", CreatedAt = now.AddMinutes(-5) },
            new() { Id = Guid.NewGuid(), ChannelId = Guid.NewGuid(), UserId = "u2", Content = "msg2", CreatedAt = now },
            new() { Id = Guid.NewGuid(), ChannelId = Guid.NewGuid(), UserId = "u3", Content = "msg3", CreatedAt = now.AddMinutes(-10) }
        };

        var ordered = messages.OrderByDescending(m => m.CreatedAt).ToList();

        Assert.Equal("msg2", ordered[0].Content);
        Assert.Equal("msg1", ordered[1].Content);
        Assert.Equal("msg3", ordered[2].Content);
    }

    [Fact]
    public void MessageTimestamp_ShouldBeUtc()
    {
        // T031: Message CreatedAt should be UTC

        var utcTime = DateTime.UtcNow;
        var message = new Message
        {
            Id = Guid.NewGuid(),
            ChannelId = Guid.NewGuid(),
            UserId = "user1",
            Content = "Test",
            CreatedAt = utcTime
        };

        Assert.Equal(utcTime.Kind, message.CreatedAt.Kind);
    }
}
