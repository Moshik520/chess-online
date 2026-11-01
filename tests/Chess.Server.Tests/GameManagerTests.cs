using Chess.Server.Games;

namespace Chess.Server.Tests;

public class GameManagerTests
{
    [Fact]
    public void FindGame_CreatedGame_ReturnsSameGame()
    {
        // Arrange
        var manager = new GameManager();
        Game created = manager.CreateGame("player-1");

        // Act
        Game? found = manager.FindGame(created.Id);

        // Assert
        Assert.Same(created, found);
    }

    [Fact]
    public void FindGame_UnknownId_ReturnsNull()
    {
        var manager = new GameManager();

        Assert.Null(manager.FindGame("no-such-game"));
    }

    [Fact]
    public void CreateGame_TwoGames_GetDifferentIds()
    {
        var manager = new GameManager();

        Game first = manager.CreateGame("player-1");
        Game second = manager.CreateGame("player-2");

        Assert.NotEqual(first.Id, second.Id);
    }

    [Fact]
    public void CreateGame_SetsCreatorAsWhite()
    {
        var manager = new GameManager();

        Game game = manager.CreateGame("player-1");

        Assert.Equal("player-1", game.WhitePlayerId);
        Assert.False(game.IsStarted);
    }
}
