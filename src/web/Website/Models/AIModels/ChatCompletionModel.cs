namespace DadABase.Web.Models.AIModels;

/// <summary>
/// Chat Completion Configuration
/// </summary>
public class ChatCompletionConfiguration
{
    /// <summary>
    /// Model
    /// </summary>
    [ConfigurationKeyName("model")]
    public string Model { get; set; }

    /// <summary>
    /// Messages
    /// </summary>
    [ConfigurationKeyName("messages")]
    public List<ChatCompletionMessage> Messages { get; set; }

    /// <summary>
    /// Max Tokens
    /// </summary>
    [ConfigurationKeyName("max_tokens")]
    public int MaxTokens { get; set; }

    /// <summary>
    /// Temperature
    /// </summary>
    [ConfigurationKeyName("temperature")]
    public float Temperature { get; set; }

    /// <summary>
    /// Top P
    /// </summary>
    [ConfigurationKeyName("top_p")]
    public float TopP { get; set; }
}

/// <summary>
/// Chat Completion Message
/// </summary>
public class ChatCompletionMessage
{
    /// <summary>
    /// Role
    /// </summary>
    [ConfigurationKeyName("role")]
    public required string Role { get; set; }

    /// <summary>
    /// Content
    /// </summary>
    [ConfigurationKeyName("content")]
    public string Content { get; set; }
}
