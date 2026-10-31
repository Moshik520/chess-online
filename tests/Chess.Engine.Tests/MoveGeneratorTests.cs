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
    public void GetLegalMoves_StartingPosition_ReturnsTwentyMoves()
    {
        var board = Board.FromFen(Board.StartingFen);

        var moves = MoveGenerator.GetLegalMoves(board);

        Assert.Equal(20, moves.Count);
    }

    [Fact]
    public void GetLegalMoves_KingCannotStepIntoCheck()
    {
        // Black rook on d8 controls the whole d-file.
        var board = Board.FromFen("3r4/8/8/8/8/8/8/4K3 w - - 0 1");
        var e1 = new Square(0, 4);

        var moves = MoveGenerator.GetLegalMoves(board);

        Assert.Equal(3, moves.Count);
        Assert.DoesNotContain(new Move(e1, new Square(0, 3)), moves);
        Assert.DoesNotContain(new Move(e1, new Square(1, 3)), moves);
    }

    [Fact]
    public void GetLegalMoves_PinnedPiece_CannotMove()
    {
        // White bishop on e2 stands between the black rook on e8 and the white king on e1.
        var board = Board.FromFen("4r3/8/8/8/8/8/4B3/4K3 w - - 0 1");
        var e2 = new Square(1, 4);

        var moves = MoveGenerator.GetLegalMoves(board);

        Assert.DoesNotContain(moves, move => move.From == e2);
    }

    [Fact]
    public void GetLegalMoves_InCheck_OnlyMovesThatEscapeCheck()
    {
        // The pawn on d2 cannot block the e-file, so only the king can move.
        var board = Board.FromFen("4r3/8/8/8/8/8/3P4/4K3 w - - 0 1");
        var e1 = new Square(0, 4);

        var moves = MoveGenerator.GetLegalMoves(board);

        Assert.Equal(3, moves.Count);
        Assert.All(moves, move => Assert.Equal(e1, move.From));
    }

    [Fact]
    public void GetLegalMoves_Checkmate_ReturnsNoMoves()
    {
        // Back-rank mate: the black king is trapped behind its own pawns.
        var board = Board.FromFen("R5k1/5ppp/8/8/8/8/8/6K1 b - - 0 1");

        var moves = MoveGenerator.GetLegalMoves(board);

        Assert.Empty(moves);
        Assert.True(MoveGenerator.IsInCheck(board, PieceColor.Black));
    }

    [Theory]
    [InlineData("r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 0 1", true, true)]
    [InlineData("r3k2r/8/8/8/8/8/8/R3K2R w - - 0 1", false, false)]      // no rights left
    [InlineData("r3k2r/8/8/8/8/8/8/RN2K2R w KQkq - 0 1", true, false)]   // knight in the way on b1
    [InlineData("4k3/4r3/8/8/8/8/8/R3K2R w KQ - 0 1", false, false)]     // cannot castle out of check
    [InlineData("5rk1/8/8/8/8/8/8/R3K2R w KQ - 0 1", false, true)]       // f1 attacked: king would pass through
    [InlineData("6rk/8/8/8/8/8/8/R3K2R w KQ - 0 1", false, true)]        // g1 attacked: king would land in check
    [InlineData("1r2k3/8/8/8/8/8/8/R3K2R w KQ - 0 1", true, true)]       // b1 attacked is fine: only the rook passes
    [InlineData("4k3/8/8/8/8/8/4p3/R3K2R w KQ - 0 1", false, false)]     // pawn on e2 attacks d1 and f1
    public void GetLegalMoves_Castling_RespectsAllConditions(string fen, bool kingside, bool queenside)
    {
        var board = Board.FromFen(fen);
        var e1 = new Square(0, 4);

        var moves = MoveGenerator.GetLegalMoves(board);

        Assert.Equal(kingside, moves.Contains(new Move(e1, new Square(0, 6))));
        Assert.Equal(queenside, moves.Contains(new Move(e1, new Square(0, 2))));
    }

    [Fact]
    public void GetLegalMoves_BlackCastling_UsesEighthRank()
    {
        var board = Board.FromFen("r3k2r/8/8/8/8/8/8/4K3 b kq - 0 1");
        var e8 = new Square(7, 4);

        var moves = MoveGenerator.GetLegalMoves(board);

        Assert.Contains(new Move(e8, new Square(7, 6)), moves);
        Assert.Contains(new Move(e8, new Square(7, 2)), moves);
    }

    [Theory]
    [InlineData(Board.StartingFen, GameStatus.InProgress)]
    [InlineData("4r3/8/8/8/8/8/3P4/4K3 w - - 0 1", GameStatus.InProgress)] // in check, but the king can escape
    [InlineData("R5k1/5ppp/8/8/8/8/8/6K1 b - - 0 1", GameStatus.Checkmate)]
    [InlineData("7k/5Q2/6K1/8/8/8/8/8 b - - 0 1", GameStatus.Stalemate)]
    public void GetGameStatus_ReturnsExpectedStatus(string fen, GameStatus expected)
    {
        var board = Board.FromFen(fen);

        Assert.Equal(expected, MoveGenerator.GetGameStatus(board));
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
    public void GetPawnMoves_EnPassantSquare_CanCaptureOntoIt()
    {
        var board = Board.FromFen("4k3/8/8/3pP3/8/8/8/4K3 w - d6 0 1");
        var e5 = new Square(4, 4);

        var moves = MoveGenerator.GetPawnMoves(board, e5);

        Assert.Contains(new Move(e5, new Square(5, 3)), moves);
    }

    [Fact]
    public void GetPawnMoves_NoEnPassantSquare_CannotCaptureDiagonallyOntoEmptySquare()
    {
        // Same position, but the black pawn reached d5 earlier, so the chance has passed.
        var board = Board.FromFen("4k3/8/8/3pP3/8/8/8/4K3 w - - 0 1");
        var e5 = new Square(4, 4);

        var moves = MoveGenerator.GetPawnMoves(board, e5);

        Assert.DoesNotContain(new Move(e5, new Square(5, 3)), moves);
    }

    [Fact]
    public void GetLegalMoves_EnPassantExposingKing_IsNotAllowed()
    {
        // Both pawns leave the 5th rank at once, which would open it for the black rook.
        var board = Board.FromFen("8/8/8/K2pP2r/8/8/8/4k3 w - d6 0 1");
        var e5 = new Square(4, 4);

        var moves = MoveGenerator.GetLegalMoves(board);

        Assert.DoesNotContain(new Move(e5, new Square(5, 3)), moves);
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
