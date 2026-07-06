//-----------------------------------------------------------------------
// <copyright file="JokeImageController.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Joke image API Controller
// </summary>
//-----------------------------------------------------------------------
namespace DadABase.API;

using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Serves joke images from private blob storage using the app's Managed Identity.
/// </summary>
[Route("api/images")]
[ApiController]
[AllowAnonymous]
public class JokeImageController : ControllerBase
{
    private const string BlobContainerName = "joke-images";
    private readonly IConfiguration configuration;
    private readonly DefaultAzureCredential azureCredential;

    /// <summary>
    /// Joke image API Controller.
    /// </summary>
    /// <param name="configuration">Application configuration.</param>
    /// <param name="azureCredential">Azure credential for Managed Identity access.</param>
    public JokeImageController(IConfiguration configuration, DefaultAzureCredential azureCredential)
    {
        this.configuration = configuration;
        this.azureCredential = azureCredential;
    }

    /// <summary>
    /// Get one generated joke image.
    /// </summary>
    /// <param name="jokeId">Joke ID.</param>
    /// <returns>Image file if it exists.</returns>
    [HttpGet("jokes/{jokeId:int}.png")]
    public async Task<IActionResult> GetJokeImage(int jokeId)
    {
        if (jokeId <= 0)
        {
            return NotFound();
        }

        var blobStorageAccountName = configuration["AppSettings:BlobStorageAccountName"];
        if (string.IsNullOrWhiteSpace(blobStorageAccountName))
        {
            return NotFound();
        }

        var blobServiceClient = new BlobServiceClient(new Uri($"https://{blobStorageAccountName}.blob.core.windows.net"), azureCredential);
        var containerClient = blobServiceClient.GetBlobContainerClient(BlobContainerName);
        var blobClient = containerClient.GetBlobClient($"{jokeId}.png");

        if (!await blobClient.ExistsAsync())
        {
            return NotFound();
        }

        var download = await blobClient.DownloadStreamingAsync();
        var contentType = string.IsNullOrWhiteSpace(download.Value.Details.ContentType)
            ? "image/png"
            : download.Value.Details.ContentType;

        return File(download.Value.Content, contentType);
    }
}