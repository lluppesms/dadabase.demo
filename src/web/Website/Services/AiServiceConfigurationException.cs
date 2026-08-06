namespace DadABase.Web.Services;

/// <summary>
/// Represents missing or invalid AI service configuration.
/// </summary>
public class AiServiceConfigurationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AiServiceConfigurationException"/> class.
    /// </summary>
    public AiServiceConfigurationException(string message) : base(message)
    {
    }
}
