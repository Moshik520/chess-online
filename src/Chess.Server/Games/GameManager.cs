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
    }

