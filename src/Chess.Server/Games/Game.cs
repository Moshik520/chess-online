using Chess.Engine;

namespace Chess.Server.Games;

public class Game
{
    public Game(string id, string whitePlayerId)
    {
        Id = id;
        WhitePlayerId = whitePlayerId;
    }

    public string Id { get; }
    public Board Board { get; } = Board.FromFen(Board.StartingFen);
    public string WhitePlayerId { get; }
    public string? BlackPlayerId { get; private set; }

    public bool IsStarted => BlackPlayerId is not null;
    public GameStatus Status => MoveGenerator.GetGameStatus(Board);

    public bool TryJoin(string playerId)
    {

        lock (_lock)
        {
            if (IsStarted || playerId == WhitePlayerId)
            {
                return false;
            }

            BlackPlayerId = playerId;
            return true;
        }
    }

   

    public MoveResult TryMakeMove(string playerId, Move move)
    {
        lock (_lock)
        {
            if (!IsStarted)
            {
                return MoveResult.GameNotStarted;
            }

            if (Status != GameStatus.InProgress)
            {
                return MoveResult.GameOver;
            }

            PieceColor? color = ColorOf(playerId);
            if (color is null)
            {
                return MoveResult.NotAPlayer;
            }

            if (color != Board.SideToMove)
            {
                return MoveResult.NotYourTurn;
            }

            if (!MoveGenerator.GetLegalMoves(Board).Contains(move))
            {
                return MoveResult.IllegalMove;
            }

            Board.MakeMove(move);
            return MoveResult.Ok;
        }
    }
    

    private PieceColor? ColorOf(string playerId)
    {
        if (playerId == WhitePlayerId)
        {
            return PieceColor.White;
        }

        if (playerId == BlackPlayerId)
        {
            return PieceColor.Black;
        }

        return null;
    }

    private readonly Lock _lock = new();
}
