using System.Collections.Concurrent;

namespace Chess.Server.Games;

public class GameManager
{
    private readonly ConcurrentDictionary<string, Game> _games = new();

    public Game CreateGame(string whitePlayerId)
    {
        while (true)
        {
            var game = new Game(NewGameId(), whitePlayerId);
            if (_games.TryAdd(game.Id, game))
            {
                return game;
            }
        }
    }

    public Game? FindGame(string gameId) =>
    _games.TryGetValue(gameId, out Game? game) ? game : null;

    private static string NewGameId() => Guid.NewGuid().ToString("N")[..8];

    public Game? FindGameByPlayer(string playerId)
    {
        return _games.Values.FirstOrDefault(game =>
            game.WhitePlayerId == playerId || game.BlackPlayerId == playerId);
    }

    public void RemoveGame(string gameId) =>
    _games.TryRemove(gameId, out _);
}
