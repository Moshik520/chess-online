using Chess.Engine;
using Chess.Server.Games;

namespace Chess.Server.Tests;

public class GameTests
{
    private const string White = "white-player";
    private const string Black = "black-player";

    private static Game StartedGame()
    {
        var game = new Game("game-1", White);
        game.TryJoin(Black);
        return game;
    }

    // "e2e4" -> Move(e2, e4)
    private static Move M(string text) => new(
        new Square(text[1] - '1', text[0] - 'a'),
        new Square(text[3] - '1', text[2] - 'a'));

    [Fact]
    public void TryJoin_SecondPlayer_StartsTheGame()
    {
        var game = new Game("game-1", White);

        bool joined = game.TryJoin(Black);

        Assert.True(joined);
        Assert.True(game.IsStarted);
        Assert.Equal(Black, game.BlackPlayerId);
    }

    [Fact]
    public void TryJoin_CreatorJoinsOwnGame_IsRejected()
    {
        var game = new Game("game-1", White);

        Assert.False(game.TryJoin(White));
        Assert.False(game.IsStarted);
    }

    [Fact]
    public void TryJoin_GameAlreadyFull_IsRejected()
    {
        var game = StartedGame();

        Assert.False(game.TryJoin("someone-else"));
        Assert.Equal(Black, game.BlackPlayerId);
    }

    [Fact]
    public void TryMakeMove_BeforeOpponentJoins_ReturnsGameNotStarted()
    {
        var game = new Game("game-1", White);

        Assert.Equal(MoveResult.GameNotStarted, game.TryMakeMove(White, M("e2e4")));
    }

    [Fact]
    public void TryMakeMove_Outsider_ReturnsNotAPlayer()
    {
        var game = StartedGame();

        Assert.Equal(MoveResult.NotAPlayer, game.TryMakeMove("someone-else", M("e2e4")));
    }

    [Fact]
    public void TryMakeMove_BlackMovesFirst_ReturnsNotYourTurn()
    {
        var game = StartedGame();

        Assert.Equal(MoveResult.NotYourTurn, game.TryMakeMove(Black, M("e7e5")));
    }

    [Fact]
    public void TryMakeMove_IllegalMove_ReturnsIllegalMoveAndLeavesBoardUnchanged()
    {
        var game = StartedGame();

        var result = game.TryMakeMove(White, M("e2e5"));

        Assert.Equal(MoveResult.IllegalMove, result);
        Assert.Equal(PieceColor.White, game.Board.SideToMove);
        Assert.NotNull(game.Board[new Square(1, 4)]);
    }

    [Fact]
    public void TryMakeMove_LegalMove_IsPlayedAndPassesTurn()
    {
        var game = StartedGame();

        var result = game.TryMakeMove(White, M("e2e4"));

        Assert.Equal(MoveResult.Ok, result);
        Assert.Null(game.Board[new Square(1, 4)]);
        Assert.Equal(PieceColor.Black, game.Board.SideToMove);
    }

    [Fact]
    public void TryMakeMove_AfterCheckmate_ReturnsGameOver()
    {
        // Fool's mate: the fastest possible checkmate.
        var game = StartedGame();
        game.TryMakeMove(White, M("f2f3"));
        game.TryMakeMove(Black, M("e7e5"));
        game.TryMakeMove(White, M("g2g4"));
        game.TryMakeMove(Black, M("d8h4"));

        Assert.Equal(GameStatus.Checkmate, game.Status);
        Assert.Equal(MoveResult.GameOver, game.TryMakeMove(White, M("a2a3")));
    }
}
