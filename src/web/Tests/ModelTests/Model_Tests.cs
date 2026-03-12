//-----------------------------------------------------------------------
// <copyright file="Model_Tests.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Model and Service Tests
// </summary>
//-----------------------------------------------------------------------
namespace DadABase.Tests;

using DadABase.Web.Models.AIModels;
using DadABase.Web.Repositories;

/// <summary>
/// Tests for model classes and services to ensure coverage.
/// </summary>
[ExcludeFromCodeCoverage]
public class Model_Tests : BaseTest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Model_Tests"/> class.
    /// </summary>
    public Model_Tests(ITestOutputHelper output)
    {
        Task.Run(() => SetupInitialize(output)).Wait();
    }

    /// <summary>
    /// Verifies ChatCompletionConfiguration properties can be set and read.
    /// </summary>
    [Fact]
    public void ChatCompletionConfiguration_Properties_Work()
    {
        // Arrange & Act
        var config = new ChatCompletionConfiguration
        {
            Model = "gpt-4o",
            Messages = new List<ChatCompletionMessage>
            {
                new ChatCompletionMessage { Role = "user", Content = "Tell me a joke" }
            },
            MaxCompletionTokens = 300,
            Temperature = 0.7f,
            TopP = 0.95f
        };

        // Assert
        Assert.Equal("gpt-4o", config.Model);
        Assert.Single(config.Messages);
        Assert.Equal("user", config.Messages[0].Role);
        Assert.Equal("Tell me a joke", config.Messages[0].Content);
        Assert.Equal(300, config.MaxCompletionTokens);
        Assert.Equal(0.7f, config.Temperature);
        Assert.Equal(0.95f, config.TopP);
    }

    /// <summary>
    /// Verifies ChatCompletionConfiguration with null optional fields does not throw.
    /// </summary>
    [Fact]
    public void ChatCompletionConfiguration_NullMaxTokens_Works()
    {
        // Arrange & Act
        var config = new ChatCompletionConfiguration
        {
            Model = "gpt-4o",
            Messages = new List<ChatCompletionMessage>(),
            MaxCompletionTokens = null
        };

        // Assert
        Assert.Null(config.MaxCompletionTokens);
        Assert.Equal("gpt-4o", config.Model);
    }

    /// <summary>
    /// Verifies ChatCompletionMessage properties can be set and read.
    /// </summary>
    [Fact]
    public void ChatCompletionMessage_Properties_Work()
    {
        // Arrange & Act
        var message = new ChatCompletionMessage { Role = "assistant", Content = "Here's a joke!" };

        // Assert
        Assert.Equal("assistant", message.Role);
        Assert.Equal("Here's a joke!", message.Content);
    }

    /// <summary>
    /// Verifies ChatCompletionMessage can hold a system role.
    /// </summary>
    [Fact]
    public void ChatCompletionMessage_SystemRole_Works()
    {
        // Arrange & Act
        var message = new ChatCompletionMessage { Role = "system", Content = "You are a helpful assistant." };

        // Assert
        Assert.Equal("system", message.Role);
        Assert.Equal("You are a helpful assistant.", message.Content);
    }

    /// <summary>
    /// Verifies ThemeService notification works when a subscriber is registered.
    /// </summary>
    [Fact]
    public void ThemeService_NotifyThemeChanged_Works()
    {
        // Arrange
        var service = new ThemeService();
        var notified = false;
        service.OnThemeChanged += () => notified = true;

        // Act
        service.NotifyThemeChanged();

        // Assert
        Assert.True(notified);
    }

    /// <summary>
    /// Verifies ThemeService works with no subscribers and does not throw.
    /// </summary>
    [Fact]
    public void ThemeService_NotifyThemeChanged_NoSubscribers_DoesNotThrow()
    {
        // Arrange
        var service = new ThemeService();

        // Act
        var exception = Record.Exception(() => service.NotifyThemeChanged());

        // Assert
        Assert.Null(exception);
    }

    /// <summary>
    /// Verifies ThemeService supports multiple subscribers.
    /// </summary>
    [Fact]
    public void ThemeService_MultipleSubscribers_AllNotified()
    {
        // Arrange
        var service = new ThemeService();
        var notifyCount = 0;
        service.OnThemeChanged += () => notifyCount++;
        service.OnThemeChanged += () => notifyCount++;

        // Act
        service.NotifyThemeChanged();

        // Assert
        Assert.Equal(2, notifyCount);
    }

    /// <summary>
    /// Verifies ThemeService subscriber can be unregistered without affecting other subscribers.
    /// </summary>
    [Fact]
    public void ThemeService_UnsubscribeWorks()
    {
        // Arrange
        var service = new ThemeService();
        var notifyCount = 0;
        Action handler = () => notifyCount++;
        service.OnThemeChanged += handler;
        service.OnThemeChanged += () => notifyCount++;

        // Act - unsubscribe first handler
        service.OnThemeChanged -= handler;
        service.NotifyThemeChanged();

        // Assert - only the second subscriber was called
        Assert.Equal(1, notifyCount);
    }
}
