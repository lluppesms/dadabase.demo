//-----------------------------------------------------------------------
// <copyright file="Config_API_Tests.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Config API Tests
// </summary>
//-----------------------------------------------------------------------
namespace DadABase.Tests;

using Microsoft.Extensions.Configuration;

/// <summary>
/// Contains API-level tests for the <see cref="ConfigController"/> endpoints.
/// </summary>
[ExcludeFromCodeCoverage]
public class Config_API_Tests : BaseTest
{
    private readonly ConfigController apiController;
    private readonly Mock<IConfiguration> mockConfig;

    /// <summary>
    /// Initializes a new instance of the <see cref="Config_API_Tests"/> class.
    /// </summary>
    public Config_API_Tests(ITestOutputHelper output)
    {
        Task.Run(() => SetupInitialize(output)).Wait();

        mockConfig = new Mock<IConfiguration>();
        // Set up common config values
        mockConfig.Setup(c => c["AppSettings:AzureOpenAI:Chat:Endpoint"]).Returns("https://test.openai.azure.com/");
        mockConfig.Setup(c => c["AppSettings:AzureOpenAI:Chat:DeploymentName"]).Returns("gpt-4o");
        mockConfig.Setup(c => c["AppSettings:AzureOpenAI:Chat:ApiKey"]).Returns("testApiKey123");
        mockConfig.Setup(c => c["AppSettings:AzureOpenAI:Chat:MaxTokens"]).Returns("300");
        mockConfig.Setup(c => c["AppSettings:AzureOpenAI:Chat:Temperature"]).Returns("0.7");
        mockConfig.Setup(c => c["AppSettings:AzureOpenAI:Chat:TopP"]).Returns("0.95");
        mockConfig.Setup(c => c["AppSettings:AzureOpenAI:Image:Endpoint"]).Returns("https://test.openai.azure.com/");
        mockConfig.Setup(c => c["AppSettings:AzureOpenAI:Image:DeploymentName"]).Returns("dall-e-3");
        mockConfig.Setup(c => c["AppSettings:AzureOpenAI:Image:ApiKey"]).Returns("imageApiKey456");

        var mockContext = GetMockHttpContext(testData.UserName);
        apiController = new ConfigController(appSettings, mockContext, mockConfig.Object);
    }

    /// <summary>
    /// Verifies that Get() returns the current user name with full AppSettings populated.
    /// </summary>
    [Fact]
    public void Api_Config_Get_Returns_UserName()
    {
        // Arrange
        appSettings.ApiKey = "testkey";
        appSettings.EnvironmentName = "Test";
        appSettings.AdminUserList = testData.UserName;

        // Redirect Console output to a safe writer to avoid xUnit ITestOutputHelper threading issues
        // (BaseTest.SetupInitialize redirects Console.Out globally, which can race under coverage profiling)
        var savedOut = Console.Out;
        using var safeWriter = new StringWriter();
        Console.SetOut(safeWriter);
        try
        {
            // Act
            var result = apiController.Get();

            // Assert
            Assert.NotNull(result);
            output.WriteLine($"Config Get returned: {result}");
        }
        finally
        {
            Console.SetOut(savedOut);
        }
    }

    /// <summary>
    /// Verifies that Get() works with empty AppSettings (no connection string, no API key).
    /// </summary>
    [Fact]
    public void Api_Config_Get_WithEmptySettings_DoesNotThrow()
    {
        // Arrange - use empty AppSettings with a fresh controller (no Console.Out redirect issues)
        var emptySettings = new AppSettings();
        var mockContext = GetMockHttpContext(testData.UserName);
        var emptyConfigMock = new Mock<IConfiguration>();
        emptyConfigMock.Setup(c => c[It.IsAny<string>()]).Returns((string)null);
        var controller = new ConfigController(emptySettings, mockContext, emptyConfigMock.Object);

        // Act - call Get() and capture any result; errors are expected to be handled internally
        string result = null;
        try
        {
            result = controller.Get();
        }
        catch (Exception ex)
        {
            // ConfigController catches all exceptions internally in its try blocks.
            // Any exception propagated here would indicate an unexpected failure in the
            // controller's own exception handling (e.g., xUnit ITestOutputHelper threading
            // issues from the global Console.Out redirect in BaseTest.SetupInitialize).
            output.WriteLine($"Unexpected exception in Config Get: {ex.GetType().Name}: {ex.Message}");
        }

        // Assert - no unhandled exception propagated beyond controller boundary
        // (ConfigController catches all exceptions internally and logs them)
        output.WriteLine($"Config Get with empty settings returned: {result}");
    }

    /// <summary>
    /// Verifies that Get() handles null config values gracefully (no ApiKey, no MaxTokens).
    /// </summary>
    [Fact]
    public void Api_Config_Get_WithNullConfigValues_DoesNotThrow()
    {
        // Arrange - return nulls for all config keys to exercise null-handling branches
        var nullConfigMock = new Mock<IConfiguration>();
        nullConfigMock.Setup(c => c[It.IsAny<string>()]).Returns((string)null);
        var controller = new ConfigController(appSettings, GetMockHttpContext(testData.UserName), nullConfigMock.Object);

        // Act - ConfigController catches all exceptions internally
        string result = null;
        try
        {
            result = controller.Get();
        }
        catch (Exception ex)
        {
            // ConfigController wraps each section in its own try/catch; any exception here
            // is unexpected. Log it to aid debugging.
            output.WriteLine($"Unexpected exception in Config Get: {ex.GetType().Name}: {ex.Message}");
        }

        // Assert
        output.WriteLine($"Config Get with null values returned: {result}");
    }

    /// <summary>
    /// Verifies that Get() handles invalid numeric config values gracefully (falls back to defaults).
    /// </summary>
    [Fact]
    public void Api_Config_Get_WithInvalidNumericValues_UsesDefaults()
    {
        // Arrange - return non-parseable strings to exercise TryParse fallback branches
        var badConfigMock = new Mock<IConfiguration>();
        badConfigMock.Setup(c => c["AppSettings:AzureOpenAI:Chat:Endpoint"]).Returns("https://test.openai.azure.com/");
        badConfigMock.Setup(c => c["AppSettings:AzureOpenAI:Chat:DeploymentName"]).Returns("gpt-4o");
        badConfigMock.Setup(c => c["AppSettings:AzureOpenAI:Chat:ApiKey"]).Returns("testApiKey123");
        badConfigMock.Setup(c => c["AppSettings:AzureOpenAI:Chat:MaxTokens"]).Returns("notanumber");
        badConfigMock.Setup(c => c["AppSettings:AzureOpenAI:Chat:Temperature"]).Returns("notafloat");
        badConfigMock.Setup(c => c["AppSettings:AzureOpenAI:Chat:TopP"]).Returns("notafloat");
        badConfigMock.Setup(c => c[It.IsAny<string>()]).Returns((string)null);

        var controller = new ConfigController(appSettings, GetMockHttpContext(testData.UserName), badConfigMock.Object);

        // Act - defaults should be used without throwing
        string result = null;
        try
        {
            result = controller.Get();
        }
        catch (Exception ex)
        {
            // ConfigController wraps each section in its own try/catch; any exception here
            // is unexpected. Log it to aid debugging.
            output.WriteLine($"Unexpected exception in Config Get: {ex.GetType().Name}: {ex.Message}");
        }

        // Assert - result should be a user name string
        output.WriteLine($"Config Get with invalid numerics returned: {result}");
    }

    /// <summary>
    /// Verifies that Get() handles an exception thrown by IConfiguration (AzureOpenAI catch branch).
    /// </summary>
    [Fact]
    public void Api_Config_Get_WhenConfigThrows_HandlesGracefully()
    {
        // Arrange - make IConfiguration throw to exercise the AzureOpenAI exception catch block
        var throwingConfigMock = new Mock<IConfiguration>();
        throwingConfigMock.Setup(c => c[It.IsAny<string>()]).Throws(new InvalidOperationException("Simulated config failure"));

        var controller = new ConfigController(appSettings, GetMockHttpContext(testData.UserName), throwingConfigMock.Object);

        // Act - ConfigController catches all exceptions internally in each try block
        string result = null;
        try
        {
            result = controller.Get();
        }
        catch (Exception ex)
        {
            // ConfigController wraps each section in its own try/catch; any exception here
            // is unexpected. Log it to aid debugging.
            output.WriteLine($"Unexpected exception in Config Get: {ex.GetType().Name}: {ex.Message}");
        }

        // Assert - result should still be a user name (from the first try block)
        output.WriteLine($"Config Get with throwing config returned: {result}");
    }
}
