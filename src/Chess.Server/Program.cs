using Chess.Server.Games;
using Chess.Server.Hubs;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<GameManager>();
builder.Services.AddSignalR();

var app = builder.Build();
app.MapHub<GameHub>("/hubs/game");

app.Run();
