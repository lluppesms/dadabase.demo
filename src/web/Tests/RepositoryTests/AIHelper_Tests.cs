//-----------------------------------------------------------------------
// <copyright file="AIHelper_Tests.cs" company="Luppes Consulting, Inc.">
// Copyright 2025, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// AI Helper Tests
// </summary>
//-----------------------------------------------------------------------
namespace DadABase.Tests.RepositoryTests;

using DadABase.Web.Repositories;
using Microsoft.Extensions.Configuration;

/// <summary>
/// AI Helper Tests
/// </summary>
[ExcludeFromCodeCoverage]
public class AIHelper_Tests
{
    /// <summary>
    /// Test GetJokeImagePath returns empty string when no image exists
    /// </summary>
    [Fact]
    public void GetJokeImagePath_NoImageExists_ReturnsEmptyString()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["AppSettings:AzureOpenAI:Chat:Endpoint"] = "https://test.endpoint.com",
                ["AppSettings:AzureOpenAI:Chat:DeploymentName"] = "test-model",
                ["AppSettings:AzureOpenAI:Image:Endpoint"] = "https://test.endpoint.com",
                ["AppSettings:AzureOpenAI:Image:DeploymentName"] = "test-image-model"
            })
            .Build();

        var mockEnvironment = new Mock<IWebHostEnvironment>();
        var tempPath = Path.Combine(Path.GetTempPath(), "dadabase-test-" + Guid.NewGuid().ToString());
        var wwwrootPath = Path.Combine(tempPath, "wwwroot");
        Directory.CreateDirectory(Path.Combine(wwwrootPath, "images", "jokes"));
        mockEnvironment.Setup(e => e.WebRootPath).Returns(wwwrootPath);

        var aiHelper = new AIHelper(configuration, mockEnvironment.Object);

        try
        {
            // Act
            var result = aiHelper.GetJokeImagePath(999999);

            // Assert
            Assert.Equal(string.Empty, result);
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
        }
    }

    /// <summary>
    /// Test GetJokeImagePath returns correct path when image exists
    /// </summary>
    [Fact]
    public void GetJokeImagePath_ImageExists_ReturnsCorrectPath()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["AppSettings:AzureOpenAI:Chat:Endpoint"] = "https://test.endpoint.com",
                ["AppSettings:AzureOpenAI:Chat:DeploymentName"] = "test-model",
                ["AppSettings:AzureOpenAI:Image:Endpoint"] = "https://test.endpoint.com",
                ["AppSettings:AzureOpenAI:Image:DeploymentName"] = "test-image-model"
            })
            .Build();

        var mockEnvironment = new Mock<IWebHostEnvironment>();
        var tempPath = Path.Combine(Path.GetTempPath(), "dadabase-test-" + Guid.NewGuid().ToString());
        var wwwrootPath = Path.Combine(tempPath, "wwwroot");
        var imageFolder = Path.Combine(wwwrootPath, "images", "jokes");
        Directory.CreateDirectory(imageFolder);
        
        // Create a test image file
        var testJokeId = 123;
        var testImagePath = Path.Combine(imageFolder, $"{testJokeId}.png");
        File.WriteAllBytes(testImagePath, new byte[] { 0x89, 0x50, 0x4E, 0x47 }); // PNG header bytes

        mockEnvironment.Setup(e => e.WebRootPath).Returns(wwwrootPath);

        var aiHelper = new AIHelper(configuration, mockEnvironment.Object);

        try
        {
            // Act
            var result = aiHelper.GetJokeImagePath(testJokeId);

            // Assert
            Assert.Equal($"/images/jokes/{testJokeId}.png", result);
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
        }
    }

    /// <summary>
    /// Test GetJokeImagePath returns empty string for invalid joke ID
    /// </summary>
    [Fact]
    public void GetJokeImagePath_InvalidJokeId_ReturnsEmptyString()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["AppSettings:AzureOpenAI:Chat:Endpoint"] = "https://test.endpoint.com",
                ["AppSettings:AzureOpenAI:Chat:DeploymentName"] = "test-model",
                ["AppSettings:AzureOpenAI:Image:Endpoint"] = "https://test.endpoint.com",
                ["AppSettings:AzureOpenAI:Image:DeploymentName"] = "test-image-model"
            })
            .Build();

        var mockEnvironment = new Mock<IWebHostEnvironment>();
        var tempPath = Path.Combine(Path.GetTempPath(), "dadabase-test-" + Guid.NewGuid().ToString());
        var wwwrootPath = Path.Combine(tempPath, "wwwroot");
        Directory.CreateDirectory(Path.Combine(wwwrootPath, "images", "jokes"));
        mockEnvironment.Setup(e => e.WebRootPath).Returns(wwwrootPath);

        var aiHelper = new AIHelper(configuration, mockEnvironment.Object);

        try
        {
            // Act
            var result = aiHelper.GetJokeImagePath(0);

            // Assert
            Assert.Equal(string.Empty, result);
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
        }
    }
}
