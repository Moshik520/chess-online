using Chess.Engine;
using Chess.Server.Games;
using Microsoft.AspNetCore.SignalR;

namespace Chess.Server.Hubs;

public class GameHub : Hub
{
    private readonly GameManager _games;

    public GameHub(GameManager games)
    {
        _games = games;
    }

    public async Task<string> CreateGame()
    {
        Game game = _games.CreateGame(Context.ConnectionId);
        await Groups.AddToGroupAsync(Context.ConnectionId, game.Id);

        return game.Id;


    }

    public async Task JoinGame(string gameId)
    {
        Game? game = _games.FindGame(gameId);

        if(game is null)
        {
            throw new HubException("Game not found");

        }
        if(!game.TryJoin(Context.ConnectionId))
        {
            throw new HubException("Cannot join this game");

        }
        await Groups.AddToGroupAsync(Context.ConnectionId, game.Id);

        await Clients.Group(game.Id).SendAsync("GameStarted",game.Board.ToFen());


    }

    public async Task MakeMove(string gameId, string uci)
    {
        Game? game = _games.FindGame(gameId);
        if (game is null)
        {
            throw new HubException("Game not found");

        }
     

        Move move;
        try
        {
            move = Move.FromUci(uci);
        }
        catch (ArgumentException)
        {
            throw new HubException($"Invalid move format '{uci}'");
        }


        MoveResult moveResult = game.TryMakeMove(Context.ConnectionId, move);
        if (moveResult != MoveResult.Ok)
        {
            throw new HubException(moveResult.ToString());

        }

        await Clients.Group(game.Id).SendAsync("MoveMade", uci, game.Board.ToFen());

        if(game.Status != GameStatus.InProgress)
        {

            await Clients.Group(game.Id).SendAsync("GameOver", game.Status.ToString());
        }

    }


}
