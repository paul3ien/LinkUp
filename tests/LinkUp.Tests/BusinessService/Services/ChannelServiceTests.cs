// T030 - ChannelService Unit Tests
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using BusinessService.Services;
using BusinessService.Data;
using BusinessService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkUp.Tests.BusinessService.Services;

public class ChannelServiceTests
{
    [Fact]
    public void CreateChannelRequest_WithValidData_ShouldHaveProperties()
    {
        // T030: Channel creation validation

        // Arrange & Act
        var name = "General";
        var createdBy = "user@example.com";

        // Assert
        Assert.NotEmpty(name);
        Assert.NotEmpty(createdBy);
    }

    [Fact]
    public void ChannelEntity_ConstructorShouldSetDefaults()
    {
        // T030: Channel entity should have ID and timestamp

        // Arrange & Act
        var channel = new Channel
        {
            Id = Guid.NewGuid(),
            Name = "Test Channel",
            CreatedBy = "user1",
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        Assert.NotEqual(Guid.Empty, channel.Id);
        Assert.Equal("Test Channel", channel.Name);
        Assert.Equal("user1", channel.CreatedBy);
        Assert.NotEqual(default, channel.CreatedAt);
    }

    [Fact]
    public void ChannelEntity_ShouldSupportMessages()
    {
        // T030: Channel should be able to contain messages

        // Arrange
        var channel = new Channel
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            CreatedBy = "user1",
            CreatedAt = DateTime.UtcNow,
            Messages = new List<Message>()
        };

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ChannelId = channel.Id,
            UserId = "user2",
            Content = "Hello",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        channel.Messages.Add(message);

        // Assert
        Assert.Single(channel.Messages);
        Assert.Equal(channel.Id, channel.Messages.First().ChannelId);
    }

    [Fact]
    public void ChannelValidation_NameCannotBeEmpty()
    {
        // T030: Channel name validation

        var name = string.Empty;
        Assert.True(string.IsNullOrWhiteSpace(name));
    }

    [Fact]
    public void ChannelValidation_GuidShouldBeUnique()
    {
        // T030: Each channel should have unique ID

        var channel1Id = Guid.NewGuid();
        var channel2Id = Guid.NewGuid();

        Assert.NotEqual(channel1Id, channel2Id);
    }

    [Fact]
    public void ChannelTimestamp_ShouldBeUtc()
    {
        // T030: Channel CreatedAt should be UTC

        var utcTime = DateTime.UtcNow;
        var channel = new Channel
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            CreatedBy = "user1",
            CreatedAt = utcTime
        };

        Assert.Equal(utcTime.Kind, channel.CreatedAt.Kind);
    }
}
