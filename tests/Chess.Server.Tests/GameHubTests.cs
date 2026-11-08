using System.Threading.Channels;
using Chess.Engine;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;

namespace Chess.Server.Tests;

// Runs the real server in memory and talks to it through real SignalR connections,
// the same way a browser would.
public class GameHubTests : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);
    private readonly WebApplicationFactory<Program> _factory;

    public GameHubTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateGame_ReturnsShortGameId()
    {
        await using var white = await ConnectAsync();

        string gameId = await white.Connection.InvokeAsync<string>("CreateGame");

        Assert.Equal(8, gameId.Length);
    }

    [Fact]
    public async Task JoinGame_BothPlayersReceiveStartingPosition()
    {
        await using var white = await ConnectAsync();
        await using var black = await ConnectAsync();
        string gameId = await white.Connection.InvokeAsync<string>("CreateGame");

        await black.Connection.InvokeAsync("JoinGame", gameId);

        Assert.Equal(Board.StartingFen, await white.GameStarted.Task.WaitAsync(Timeout));
        Assert.Equal(Board.StartingFen, await black.GameStarted.Task.WaitAsync(Timeout));
    }

    [Fact]
    public async Task JoinGame_UnknownId_FailsWithMessage()
    {
        await using var player = await ConnectAsync();

        var error = await Assert.ThrowsAsync<HubException>(
            () => player.Connection.InvokeAsync("JoinGame", "no-such-game"));

        Assert.Contains("Game not found", error.Message);
    }

    [Fact]
    public async Task MakeMove_BothPlayersReceiveMoveAndNewPosition()
    {
        var (white, black, gameId) = await StartGameAsync();
        await using var _ = white;
        await using var __ = black;

        await white.Connection.InvokeAsync("MakeMove", gameId, "e2e4");

        var expected = ("e2e4", "rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1");
        Assert.Equal(expected, await white.MovesMade.Reader.ReadAsync().AsTask().WaitAsync(Timeout));
        Assert.Equal(expected, await black.MovesMade.Reader.ReadAsync().AsTask().WaitAsync(Timeout));
    }

    [Fact]
    public async Task MakeMove_OutOfTurn_FailsWithReason()
    {
        var (white, black, gameId) = await StartGameAsync();
        await using var _ = white;
        await using var __ = black;

        var error = await Assert.ThrowsAsync<HubException>(
            () => black.Connection.InvokeAsync("MakeMove", gameId, "e7e5"));

        Assert.Contains("NotYourTurn", error.Message);
    }

    [Fact]
    public async Task MakeMove_BadFormat_FailsWithReason()
    {
        var (white, black, gameId) = await StartGameAsync();
        await using var _ = white;
        await using var __ = black;

        var error = await Assert.ThrowsAsync<HubException>(
            () => white.Connection.InvokeAsync("MakeMove", gameId, "xyz"));

        Assert.Contains("Invalid move format", error.Message);
    }

    [Fact]
    public async Task MakeMove_Checkmate_BothPlayersReceiveGameOver()
    {
        var (white, black, gameId) = await StartGameAsync();
        await using var _ = white;
        await using var __ = black;

        // Fool's mate
        await white.Connection.InvokeAsync("MakeMove", gameId, "f2f3");
        await black.Connection.InvokeAsync("MakeMove", gameId, "e7e5");
        await white.Connection.InvokeAsync("MakeMove", gameId, "g2g4");
        await black.Connection.InvokeAsync("MakeMove", gameId, "d8h4");

        Assert.Equal("Checkmate", await white.GameOver.Task.WaitAsync(Timeout));
        Assert.Equal("Checkmate", await black.GameOver.Task.WaitAsync(Timeout));
    }

    [Fact]
    public async Task GetLegalMoves_PawnOnStartSquare_ReturnsBothTargets()
    {
        var (white, black, gameId) = await StartGameAsync();
        await using var _ = white;
        await using var __ = black;

        var targets = await white.Connection.InvokeAsync<List<string>>("GetLegalMoves", gameId, "e2");

        Assert.Equal(["e3", "e4"], targets.Order());
    }

    [Fact]
    public async Task GetLegalMoves_BlockedPiece_ReturnsEmptyList()
    {
        var (white, black, gameId) = await StartGameAsync();
        await using var _ = white;
        await using var __ = black;

        var targets = await white.Connection.InvokeAsync<List<string>>("GetLegalMoves", gameId, "a1");

        Assert.Empty(targets);
    }

    [Fact]
    public async Task GetLegalMoves_UnknownGame_FailsWithMessage()
    {
        await using var player = await ConnectAsync();

        var error = await Assert.ThrowsAsync<HubException>(
            () => player.Connection.InvokeAsync<List<string>>("GetLegalMoves", "no-such-game", "e2"));

        Assert.Contains("Game not found", error.Message);
    }

    [Fact]
    public async Task Disconnect_OpponentIsToldAndGameIsClosed()
    {
        var (white, black, gameId) = await StartGameAsync();
        await using var _ = white;

        await black.DisposeAsync();

        await white.PlayerDisconnected.Task.WaitAsync(Timeout);
        var error = await Assert.ThrowsAsync<HubException>(
            () => white.Connection.InvokeAsync("MakeMove", gameId, "e2e4"));
        Assert.Contains("Game not found", error.Message);
    }

    [Fact]
    public async Task Checkmate_ClosesTheGame()
    {
        var (white, black, gameId) = await StartGameAsync();
        await using var _ = white;
        await using var __ = black;

        await white.Connection.InvokeAsync("MakeMove", gameId, "f2f3");
        await black.Connection.InvokeAsync("MakeMove", gameId, "e7e5");
        await white.Connection.InvokeAsync("MakeMove", gameId, "g2g4");
        await black.Connection.InvokeAsync("MakeMove", gameId, "d8h4");

        var error = await Assert.ThrowsAsync<HubException>(
            () => white.Connection.InvokeAsync("MakeMove", gameId, "a2a3"));
        Assert.Contains("Game not found", error.Message);
    }

    private async Task<(TestPlayer White, TestPlayer Black, string GameId)> StartGameAsync()
    {
        var white = await ConnectAsync();
        var black = await ConnectAsync();
        string gameId = await white.Connection.InvokeAsync<string>("CreateGame");
        await black.Connection.InvokeAsync("JoinGame", gameId);
        await white.GameStarted.Task.WaitAsync(Timeout);
        await black.GameStarted.Task.WaitAsync(Timeout);
        return (white, black, gameId);
    }

    private async Task<TestPlayer> ConnectAsync()
    {
        var connection = new HubConnectionBuilder()
            .WithUrl(new Uri(_factory.Server.BaseAddress, "hubs/game"), options =>
            {
                // The in-memory test server has no real network, so route through its handler.
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                options.Transports = HttpTransportType.LongPolling;
            })
            .Build();

        var player = new TestPlayer(connection);
        await connection.StartAsync();
        return player;
    }

    // Records every message the server pushes to one connection.
    private sealed class TestPlayer : IAsyncDisposable
    {
        public TestPlayer(HubConnection connection)
        {
            Connection = connection;
            connection.On<string>("GameStarted", fen => GameStarted.TrySetResult(fen));
            connection.On<string, string>("MoveMade", (uci, fen) => MovesMade.Writer.TryWrite((uci, fen)));
            connection.On<string>("GameOver", status => GameOver.TrySetResult(status));
            connection.On("PlayerDisconnected", () => PlayerDisconnected.TrySetResult());
        }

        public HubConnection Connection { get; }

        public TaskCompletionSource<string> GameStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Channel<(string Uci, string Fen)> MovesMade { get; } = Channel.CreateUnbounded<(string, string)>();

        public TaskCompletionSource<string> GameOver { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource PlayerDisconnected { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public ValueTask DisposeAsync() => Connection.DisposeAsync();
    }
}
