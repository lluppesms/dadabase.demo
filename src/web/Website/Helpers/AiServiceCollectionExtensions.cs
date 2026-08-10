namespace DadABase.Web.Helpers;

/// <summary>
/// Registers AI service abstractions and provider implementations.
/// </summary>
public static class AiServiceCollectionExtensions
{
    /// <summary>
    /// Adds chat and image AI services using the configured chat provider.
    /// </summary>
    public static IServiceCollection AddAiServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddSingleton<IAiImageService, AiImageService>();
        services.AddSingleton<IAiChatService>(sp =>
        {
            var provider = config["AppSettings:AiServiceProvider"];
            if (string.Equals(provider, "CopilotSDK", StringComparison.OrdinalIgnoreCase))
            {
                return new CopilotSdkChatService(
                    sp.GetRequiredService<IConfiguration>(),
                    sp.GetRequiredService<DefaultAzureCredential>(),
                    sp.GetRequiredService<ILogger<CopilotSdkChatService>>());
            }

            return new AgentFrameworkChatService(sp.GetRequiredService<IConfiguration>());
        });

        return services;
    }
}
