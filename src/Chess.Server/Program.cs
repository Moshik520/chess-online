using Chess.Engine;
using Chess.Server.Games;
using Chess.Server.Hubs;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<GameManager>();
builder.Services.AddSignalR();

var app = builder.Build();
app.MapHub<GameHub>("/hubs/game");

app.MapGet("/", () => "Hello World!");

app.MapGet("/legal-moves", (string fen) =>
{
    Board board = Board.FromFen(fen);
    return MoveGenerator.GetLegalMoves(board);
});

app.Run();
