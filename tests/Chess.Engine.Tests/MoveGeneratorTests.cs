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

    [Fact]
    public void GetBishopMoves_AloneInCenter_ReturnsThirteenMoves()
    {
        var board = Board.FromFen("8/8/8/8/3B4/8/8/8 w - - 0 1");

        var moves = MoveGenerator.GetBishopMoves(board, new Square(3, 3));

        Assert.Equal(13, moves.Count);
    }

    [Fact]
    public void GetQueenMoves_AloneInCenter_ReturnsTwentySevenMoves()
    {
        var board = Board.FromFen("8/8/8/8/3Q4/8/8/8 w - - 0 1");

        var moves = MoveGenerator.GetQueenMoves(board, new Square(3, 3));

        Assert.Equal(27, moves.Count);
    }

    [Theory]
    [InlineData("8/8/8/8/3K4/8/8/8 w - - 0 1", 3, 3, 8)]
    [InlineData("8/8/8/8/8/8/8/K7 w - - 0 1", 0, 0, 3)]
    [InlineData("8/8/8/8/8/8/PP6/KN6 w - - 0 1", 0, 0, 0)]
    public void GetKingMoves_ReturnsExpectedCount(string fen, int rank, int file, int expected)
    {
        var board = Board.FromFen(fen);

        var moves = MoveGenerator.GetKingMoves(board, new Square(rank, file));

        Assert.Equal(expected, moves.Count);
    }

    [Fact]
    public void GetKingMoves_EnemyNeighbour_CanBeCaptured()
    {
        var board = Board.FromFen("8/8/8/8/8/8/p7/K7 w - - 0 1");
        var from = new Square(0, 0);

        var moves = MoveGenerator.GetKingMoves(board, from);

        Assert.Contains(new Move(from, new Square(1, 0)), moves);
    }

    [Theory]
    [InlineData(Board.StartingFen, PieceColor.White, false)]
    [InlineData(Board.StartingFen, PieceColor.Black, false)]
    [InlineData("4k3/8/8/8/8/8/8/4R2K b - - 0 1", PieceColor.Black, true)]   // rook on the open e-file
    [InlineData("4k3/8/8/8/4p3/8/8/4R2K b - - 0 1", PieceColor.Black, false)] // own pawn blocks the rook
    [InlineData("4k3/8/3N4/8/8/8/8/7K b - - 0 1", PieceColor.Black, true)]   // knight jumps to the king
    [InlineData("4k3/3P4/8/8/8/8/8/7K b - - 0 1", PieceColor.Black, true)]   // pawn attacks diagonally
    [InlineData("4k3/4P3/8/8/8/8/8/7K b - - 0 1", PieceColor.Black, false)]  // pawn straight ahead does not
    [InlineData("4k3/8/8/8/8/8/8/4R2K b - - 0 1", PieceColor.White, false)]  // only the attacked side is in check
    public void IsInCheck_ReturnsWhetherKingIsAttacked(string fen, PieceColor color, bool expected)
    {
        var board = Board.FromFen(fen);

        Assert.Equal(expected, MoveGenerator.IsInCheck(board, color));
    }

    [Fact]
    public void GetPawnMoves_WhiteOnStartRank_CanAdvanceOneOrTwo()
    {
        var board = Board.FromFen(Board.StartingFen);
        var from = new Square(1, 4);

        var moves = MoveGenerator.GetPawnMoves(board, from);

        Assert.Equal(2, moves.Count);
        Assert.Contains(new Move(from, new Square(2, 4)), moves);
        Assert.Contains(new Move(from, new Square(3, 4)), moves);
    }

    [Fact]
    public void GetPawnMoves_BlackOnStartRank_AdvancesDownTheBoard()
    {
        var board = Board.FromFen(Board.StartingFen);
        var from = new Square(6, 4);

        var moves = MoveGenerator.GetPawnMoves(board, from);

        Assert.Equal(2, moves.Count);
        Assert.Contains(new Move(from, new Square(5, 4)), moves);
        Assert.Contains(new Move(from, new Square(4, 4)), moves);
    }

    [Fact]
    public void GetPawnMoves_OffStartRank_AdvancesOnlyOne()
    {
        var board = Board.FromFen("8/8/8/8/8/4P3/8/8 w - - 0 1");
        var from = new Square(2, 4);

        var moves = MoveGenerator.GetPawnMoves(board, from);

        Assert.Equal([new Move(from, new Square(3, 4))], moves);
    }

    [Theory]
    [InlineData("8/8/8/8/8/4p3/4P3/8 w - - 0 1")]
    [InlineData("8/8/8/8/4p3/8/4P3/8 w - - 0 1")]
    public void GetPawnMoves_BlockedAhead_CannotJumpOver(string fen)
    {
        var board = Board.FromFen(fen);
        var from = new Square(1, 4);

        var moves = MoveGenerator.GetPawnMoves(board, from);

        Assert.DoesNotContain(new Move(from, new Square(3, 4)), moves);
    }

    [Fact]
    public void GetPawnMoves_EnemyDiagonal_CanCaptureButNotOwnPiece()
    {
        var board = Board.FromFen("8/8/8/8/8/3p1N2/4P3/8 w - - 0 1");
        var from = new Square(1, 4);

        var moves = MoveGenerator.GetPawnMoves(board, from);

        Assert.Contains(new Move(from, new Square(2, 3)), moves);
        Assert.DoesNotContain(new Move(from, new Square(2, 5)), moves);
    }

    [Fact]
    public void GetPawnMoves_ReachesLastRank_OffersAllFourPromotions()
    {
        var board = Board.FromFen("8/4P3/8/8/8/8/8/8 w - - 0 1");
        var from = new Square(6, 4);
        var to = new Square(7, 4);

        var moves = MoveGenerator.GetPawnMoves(board, from);

        Assert.Equal(4, moves.Count);
        Assert.Contains(new Move(from, to, PieceType.Queen), moves);
        Assert.Contains(new Move(from, to, PieceType.Rook), moves);
        Assert.Contains(new Move(from, to, PieceType.Bishop), moves);
        Assert.Contains(new Move(from, to, PieceType.Knight), moves);
    }

    [Fact]
    public void GetPawnMoves_CaptureOnLastRank_AlsoPromotes()
    {
        var board = Board.FromFen("3rr3/4P3/8/8/8/8/8/8 w - - 0 1");
        var from = new Square(6, 4);

        var moves = MoveGenerator.GetPawnMoves(board, from);

        Assert.Equal(4, moves.Count);
        Assert.All(moves, move => Assert.Equal(new Square(7, 3), move.To));
        Assert.All(moves, move => Assert.NotNull(move.Promotion));
    }

}
