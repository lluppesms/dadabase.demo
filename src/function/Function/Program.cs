var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
              .AddEnvironmentVariables()
              .AddUserSecrets(Assembly.GetExecutingAssembly(), true);
    })
    .ConfigureFunctionsWorkerDefaults(
      builder =>
      {
          builder.UseMiddleware<MyExceptionHandler>();
      }
    )
    .ConfigureServices((context, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddSingleton<IJokeRepository, JokeRepository>();
        var configuration = context.Configuration;
        services.AddSwaggerConfiguration(configuration);
    })
    .Build();

host.Run();