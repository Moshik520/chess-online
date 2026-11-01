using Chess.Engine;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/legal-moves", (string fen) =>
{
    Board board = Board.FromFen(fen);
    return MoveGenerator.GetLegalMoves(board);
});

app.Run();
