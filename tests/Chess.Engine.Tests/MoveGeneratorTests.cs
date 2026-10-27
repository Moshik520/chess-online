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

    [Fact]
    public void GetRookMoves_BlockedAndCapture_ReturnsTenMoves()
    {
        
        var board = Board.FromFen("8/8/3P4/8/3R2p1/8/8/8 w - - 0 1");
        Square startingSquare = new Square(3, 3);
        var moves = MoveGenerator.GetRookMoves(board, startingSquare);

        Assert.Equal(10, moves.Count);
        Assert.Contains(new Move(startingSquare, new Square(3, 6)), moves);
        Assert.DoesNotContain(new Move(startingSquare, new Square(5, 3)), moves);
        Assert.DoesNotContain(new Move(startingSquare, new Square(3, 7)), moves);
    }

}
