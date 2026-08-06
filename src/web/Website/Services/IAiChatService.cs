namespace DadABase.Web.Services;

/// <summary>
/// Provides a single-turn AI chat completion.
/// </summary>
public interface IAiChatService
{
    /// <summary>
    /// Completes a user message with the supplied system prompt.
    /// </summary>
    Task<string> CompleteAsync(string systemPrompt, string userMessage, CancellationToken cancellationToken = default);
}
