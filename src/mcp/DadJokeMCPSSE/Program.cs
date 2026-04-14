
var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    //.WithPrompts<DadJokePrompts>()
    //.WithResources<DadJokeResources>()
    .WithTools<DadJokeTools>();

builder.Services.AddHttpClient();
builder.Services.AddSingleton<DadJokeService>();

var app = builder.Build();

app.MapMcp();

app.Run();