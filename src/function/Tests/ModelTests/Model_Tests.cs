//-----------------------------------------------------------------------
// <copyright file="Model_Tests.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Model and Service Tests
// </summary>
//-----------------------------------------------------------------------
namespace DadABase.Tests;

/// <summary>
/// Tests for model classes to ensure coverage.
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

    #region BuildInfo Tests

    /// <summary>
    /// Verifies BuildInfo properties can be set and read.
    /// </summary>
    [Fact]
    public void BuildInfo_Properties_Work()
    {
        // Arrange & Act
        var buildInfo = new BuildInfo
        {
            BuildDate = "2026-01-01",
            BuildNumber = "1.0.0",
            BuildId = "12345",
            BranchName = "main",
            CommitHash = "abc123def456"
        };

        // Assert
        Assert.Equal("2026-01-01", buildInfo.BuildDate);
        Assert.Equal("1.0.0", buildInfo.BuildNumber);
        Assert.Equal("12345", buildInfo.BuildId);
        Assert.Equal("main", buildInfo.BranchName);
        Assert.Equal("abc123def456", buildInfo.CommitHash);
    }

    /// <summary>
    /// Verifies BuildInfo can be constructed with default values.
    /// </summary>
    [Fact]
    public void BuildInfo_DefaultConstructor_Works()
    {
        // Arrange & Act
        var buildInfo = new BuildInfo();

        // Assert
        Assert.Null(buildInfo.BuildDate);
        Assert.Null(buildInfo.BuildNumber);
        Assert.Null(buildInfo.BuildId);
        Assert.Null(buildInfo.BranchName);
        Assert.Null(buildInfo.CommitHash);
    }

    #endregion

    #region Constants Tests

    /// <summary>
    /// Verifies Constants.Initialize sets all properties correctly.
    /// </summary>
    [Fact]
    public void Constants_Initialize_Works()
    {
        // Arrange
        var settings = new AppSettings
        {
            AppTitle = "Test Application",
            SuperUserFirstName = "Admin",
            SuperUserLastName = "User"
        };

        // Act
        Constants.Initialize(settings);

        // Assert
        Assert.Equal("Test Application", Constants.ApplicationTitle);
        Assert.Equal("Admin", Constants.SuperUserFirstName);
        Assert.Equal("User", Constants.SuperUserLastName);
        output.WriteLine($"ApplicationTitle: {Constants.ApplicationTitle}");
    }

    /// <summary>
    /// Verifies Constants.Security values are correct.
    /// </summary>
    [Fact]
    public void Constants_Security_Values_Are_Correct()
    {
        // Assert
        Assert.Equal("isAdmin", Constants.Security.AdminClaimType);
        Assert.Equal("Admin", Constants.Security.AdminRoleName);
    }

    /// <summary>
    /// Verifies Constants.LanguageModelType values are correct.
    /// </summary>
    [Fact]
    public void Constants_LanguageModelType_Values_Are_Correct()
    {
        // Assert
        Assert.Equal("text-davinci-003", Constants.LanguageModelType.textDavinci003);
        Assert.Equal("gpt35", Constants.LanguageModelType.gpt35turbo);
    }

    /// <summary>
    /// Verifies Constants.OpenAIMessages values are not null.
    /// </summary>
    [Fact]
    public void Constants_OpenAIMessages_Values_Are_Correct()
    {
        // Assert
        Assert.NotNull(Constants.OpenAIMessages.FindingJoke);
        Assert.NotNull(Constants.OpenAIMessages.StartingUp);
        Assert.NotNull(Constants.OpenAIMessages.SendingRequest);
        Assert.NotNull(Constants.OpenAIMessages.Error);
        Assert.NotNull(Constants.OpenAIMessages.Finished);
        Assert.NotNull(Constants.OpenAIMessages.Disabled);
    }

    /// <summary>
    /// Verifies Constants.OpenAIImageSize values are correct.
    /// </summary>
    [Fact]
    public void Constants_OpenAIImageSize_Values_Are_Correct()
    {
        // Assert
        Assert.Equal("256x256", Constants.OpenAIImageSize.Size256);
        Assert.Equal("512x512", Constants.OpenAIImageSize.Size512);
        Assert.Equal("1024x1024", Constants.OpenAIImageSize.Size1024);
    }

    /// <summary>
    /// Verifies Constants.LocalStorage values are correct.
    /// </summary>
    [Fact]
    public void Constants_LocalStorage_Values_Are_Correct()
    {
        // Assert
        Assert.Equal("Chat", Constants.LocalStorage.ChatSessionObject);
        Assert.Equal("SimpleChat", Constants.LocalStorage.SimpleChatSessionObject);
        Assert.Equal("NewSession", Constants.LocalStorage.NewSessionObject);
    }

    #endregion

    #region BackupData Tests

    /// <summary>
    /// Verifies BackupData default constructor initializes all lists.
    /// </summary>
    [Fact]
    public void BackupData_DefaultConstructor_Works()
    {
        // Arrange & Act
        var backupData = new BackupData();

        // Assert
        Assert.NotNull(backupData.Categories);
        Assert.NotNull(backupData.Jokes);
        Assert.NotNull(backupData.Ratings);
        Assert.Empty(backupData.Categories);
        Assert.Empty(backupData.Jokes);
        Assert.Empty(backupData.Ratings);
    }

    /// <summary>
    /// Verifies BackupData properties can be set and read.
    /// </summary>
    [Fact]
    public void BackupData_Properties_Work()
    {
        // Arrange
        var backupData = new BackupData();

        // Act
        backupData.Categories = new List<JokeCategory> { new JokeCategory { JokeCategoryId = 1, JokeCategoryTxt = "Chickens" } };
        backupData.Jokes = new List<Joke> { new Joke { JokeId = 1, JokeTxt = "A funny joke" } };
        backupData.Ratings = new List<JokeRating> { new JokeRating() };

        // Assert
        Assert.Single(backupData.Categories);
        Assert.Single(backupData.Jokes);
        Assert.Single(backupData.Ratings);
    }

    #endregion

    #region JokeBasic Tests

    /// <summary>
    /// Verifies JokeBasic default constructor works.
    /// </summary>
    [Fact]
    public void JokeBasic_DefaultConstructor_Works()
    {
        // Arrange & Act
        var joke = new JokeBasic();

        // Assert
        Assert.Equal(string.Empty, joke.Joke);
        Assert.Equal(string.Empty, joke.Category);
    }

    /// <summary>
    /// Verifies JokeBasic parameterized constructor sets properties correctly.
    /// </summary>
    [Fact]
    public void JokeBasic_ParameterizedConstructor_Works()
    {
        // Arrange & Act
        var joke = new JokeBasic("Why did the chicken cross the road?", "Chickens");

        // Assert
        Assert.Equal("Why did the chicken cross the road?", joke.Joke);
        Assert.Equal("Chickens", joke.Category);
        output.WriteLine($"Joke: {joke.Category} - {joke.Joke}");
    }

    #endregion

    #region JokeBasicPlus Tests

    /// <summary>
    /// Verifies JokeBasicPlus default constructor works.
    /// </summary>
    [Fact]
    public void JokeBasicPlus_DefaultConstructor_Works()
    {
        // Arrange & Act
        var joke = new JokeBasicPlus();

        // Assert
        Assert.Equal(string.Empty, joke.Joke);
        Assert.Equal(string.Empty, joke.Category);
        Assert.Equal(string.Empty, joke.Attribution);
    }

    /// <summary>
    /// Verifies JokeBasicPlus two-param constructor sets properties correctly.
    /// </summary>
    [Fact]
    public void JokeBasicPlus_TwoParamConstructor_Works()
    {
        // Arrange & Act
        var joke = new JokeBasicPlus("Why did the chicken cross the road?", "Chickens");

        // Assert
        Assert.Equal("Why did the chicken cross the road?", joke.Joke);
        Assert.Equal("Chickens", joke.Category);
        Assert.Null(joke.Attribution);
    }

    /// <summary>
    /// Verifies JokeBasicPlus three-param constructor with a real attribution value works.
    /// </summary>
    [Fact]
    public void JokeBasicPlus_ThreeParamConstructor_WithAttribution_Works()
    {
        // Arrange & Act
        var joke = new JokeBasicPlus("Why did the chicken cross the road?", "Chickens", "John Doe");

        // Assert
        Assert.Equal("Why did the chicken cross the road?", joke.Joke);
        Assert.Equal("Chickens", joke.Category);
        Assert.Equal("John Doe", joke.Attribution);
    }

    /// <summary>
    /// Verifies JokeBasicPlus three-param constructor with null attribution sets Attribution to null.
    /// </summary>
    [Fact]
    public void JokeBasicPlus_ThreeParamConstructor_NullAttribution_Works()
    {
        // Arrange & Act
        var joke = new JokeBasicPlus("Why did the chicken cross the road?", "Chickens", null);

        // Assert
        Assert.Null(joke.Attribution);
    }

    /// <summary>
    /// Verifies JokeBasicPlus three-param constructor with empty attribution sets Attribution to null.
    /// </summary>
    [Fact]
    public void JokeBasicPlus_ThreeParamConstructor_EmptyAttribution_Works()
    {
        // Arrange & Act
        var joke = new JokeBasicPlus("Why did the chicken cross the road?", "Chickens", string.Empty);

        // Assert
        Assert.Null(joke.Attribution);
    }

    /// <summary>
    /// Verifies JokeBasicPlus Joke-based constructor with attribution sets properties correctly.
    /// </summary>
    [Fact]
    public void JokeBasicPlus_JokeConstructor_WithAttribution_Works()
    {
        // Arrange
        var jokeEntity = new Joke { JokeTxt = "Why did the chicken cross the road?", Categories = "Chickens", Attribution = "John Doe" };

        // Act
        var joke = new JokeBasicPlus(jokeEntity);

        // Assert
        Assert.Equal("Why did the chicken cross the road?", joke.Joke);
        Assert.Equal("Chickens", joke.Category);
        Assert.Equal("John Doe", joke.Attribution);
        output.WriteLine($"Joke: {joke.Category} - {joke.Joke} ({joke.Attribution})");
    }

    /// <summary>
    /// Verifies JokeBasicPlus Joke-based constructor with empty attribution sets Attribution to null.
    /// </summary>
    [Fact]
    public void JokeBasicPlus_JokeConstructor_EmptyAttribution_Works()
    {
        // Arrange
        var jokeEntity = new Joke { JokeTxt = "Why did the chicken cross the road?", Categories = "Chickens", Attribution = string.Empty };

        // Act
        var joke = new JokeBasicPlus(jokeEntity);

        // Assert
        Assert.Equal("Why did the chicken cross the road?", joke.Joke);
        Assert.Equal("Chickens", joke.Category);
        Assert.Null(joke.Attribution);
    }

    #endregion

    #region ValueMessage Tests

    /// <summary>
    /// Verifies ValueMessage default constructor initializes with expected defaults.
    /// </summary>
    [Fact]
    public void ValueMessage_DefaultConstructor_Works()
    {
        // Arrange & Act
        var msg = new ValueMessage();

        // Assert
        Assert.Equal(-1, msg.Value);
        Assert.Equal(string.Empty, msg.Message);
    }

    /// <summary>
    /// Verifies ValueMessage string constructor sets Value=0 for non-error messages.
    /// </summary>
    [Fact]
    public void ValueMessage_StringConstructor_Normal_Works()
    {
        // Arrange & Act
        var msg = new ValueMessage("test message");

        // Assert
        Assert.Equal(0, msg.Value);
        Assert.Equal("test message", msg.Message);
    }

    /// <summary>
    /// Verifies ValueMessage string constructor sets Value=1 for error messages.
    /// </summary>
    [Fact]
    public void ValueMessage_StringConstructor_Error_Works()
    {
        // Arrange & Act
        var msg = new ValueMessage("Error occurred");

        // Assert
        Assert.Equal(1, msg.Value);
        Assert.Equal("Error occurred", msg.Message);
    }

    /// <summary>
    /// Verifies ValueMessage string constructor sets Value=1 for timeout messages.
    /// </summary>
    [Fact]
    public void ValueMessage_StringConstructor_Timeout_Works()
    {
        // Arrange & Act
        var msg = new ValueMessage("TimeOut occurred");

        // Assert
        Assert.Equal(1, msg.Value);
        Assert.Equal("TimeOut occurred", msg.Message);
    }

    /// <summary>
    /// Verifies ValueMessage two-param constructor sets both properties correctly.
    /// </summary>
    [Fact]
    public void ValueMessage_TwoParamConstructor_Works()
    {
        // Arrange & Act
        var msg = new ValueMessage("test message", 42);

        // Assert
        Assert.Equal(42, msg.Value);
        Assert.Equal("test message", msg.Message);
    }

    #endregion
}
