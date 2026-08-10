namespace DadABase.Web.Services;

/// <summary>
/// Provides image generation and image blob storage operations.
/// </summary>
public interface IAiImageService
{
    /// <summary>
    /// Generates an image for the supplied description.
    /// </summary>
    Task<(string imageDataUrl, bool success, string message)> GenerateAnImage(string imageDescription, int jokeId = 0);

    /// <summary>
    /// Saves an existing base64 image to blob storage.
    /// </summary>
    Task<(string blobUrl, bool success, string message)> SaveBase64ImageToBlob(string base64ImageDataUrl, int jokeId);

    /// <summary>
    /// Gets the image route for a joke when an image exists.
    /// </summary>
    string GetJokeImagePath(int jokeId);
}
