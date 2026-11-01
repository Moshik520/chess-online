namespace Chess.Server.Games;

public enum MoveResult
{
    Ok,
    GameNotStarted,
    GameOver,
    NotAPlayer,
    NotYourTurn,
    IllegalMove
}
