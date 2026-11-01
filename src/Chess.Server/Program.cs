using Chess.Engine;
using Chess.Server.Games;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<GameManager>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/legal-moves", (string fen) =>
{
    Board board = Board.FromFen(fen);
    return MoveGenerator.GetLegalMoves(board);
});

app.Run();
