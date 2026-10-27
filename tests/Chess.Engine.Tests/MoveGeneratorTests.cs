using Chess.Engine;

namespace Chess.Engine.Tests;

public class MoveGeneratorTests
{
    [Fact]
    public void GetKnightMoves_StartingPosition_ReturnsTwoMoves()
    {
        var board = Board.FromFen(Board.StartingFen);
        Square startingSquare = new Square(0, 1);
        var moves = MoveGenerator.GetKnightMoves(board, startingSquare);

        Assert.Equal(2, moves.Count);
        Assert.Contains(new Move(startingSquare, new Square(2, 0)), moves);
        Assert.Contains(new Move(startingSquare, new Square(2, 2)), moves);
    }

}
