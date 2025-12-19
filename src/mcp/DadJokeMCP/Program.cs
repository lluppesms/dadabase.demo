
var builder = Host.CreateEmptyApplicationBuilder(settings: null);
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<DadJokeTools>();

builder.Services.AddSingleton<DadJokeService>();

await builder.Build().RunAsync();