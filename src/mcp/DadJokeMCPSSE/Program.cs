
var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddMcpServer()
    .WithTools<DadJokeTools>();

builder.Services.AddHttpClient();
builder.Services.AddSingleton<DadJokeService>();

var app = builder.Build();

app.MapMcp();

app.Run();