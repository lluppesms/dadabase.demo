namespace DadABase.Function.Helpers;

/// <summary>
/// Swagger/OpenAPI Configuration
/// </summary>
public static class SwaggerConfig
{
    /// <summary>
    /// Adds OpenAPI/Swagger configuration to the service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IOpenApiConfigurationOptions>(_ =>
        {
            var openApiConfig = configuration.GetSection("OpenApi");

            var options = new OpenApiConfigurationOptions
            {
                Info = new OpenApiInfo
                {
                    Version = openApiConfig["Version"] ?? "v1",
                    Title = openApiConfig["Title"] ?? "Dad-A-Base Function API",
                    Description = openApiConfig["Description"] ?? "Azure Functions API for Dad Jokes",
                    Contact = new OpenApiContact
                    {
                        Name = openApiConfig["ContactName"] ?? "The Developer",
                        Url = new Uri(openApiConfig["ContactUrl"] ?? "http://mywebsite.com")
                    },
                    License = new OpenApiLicense
                    {
                        Name = openApiConfig["LicenseName"] ?? "MIT License",
                        Url = new Uri(openApiConfig["LicenseUrl"] ?? "http://mywebsite.com/license")
                    }
                },
                Servers = DefaultOpenApiConfigurationOptions.GetHostNames(),
                OpenApiVersion = OpenApiVersionType.V3,
                IncludeRequestingHostName = true,
                ForceHttps = false,
                ForceHttp = false
            };

            return options;
        });

        return services;
    }
}
