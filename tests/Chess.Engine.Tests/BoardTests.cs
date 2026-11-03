using Chess.Engine;

namespace Chess.Engine.Tests;

public class BoardTests
{
    [Fact]
    public void NewBoard_SquareIsEmpty()
    {
        // Arrange
        var board = new Board();
 

        // Assert
        Assert.Null(board[0, 0]);
    }


    [Fact]
    public void SetPiece_ReturnsSamePiece()
    {
        // Arrange
        var board = new Board();
        var piece = new Piece(PieceType.Knight, PieceColor.White);


        // Act
        board[0, 0] = piece;


        // Assert
        Assert.Equal(piece, board[0, 0]);
    }

    [Fact]
    public void FromFen_StartingPosition_PlacesKingsAndQueens()
    {
        // Act
        var board = Board.FromFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");

        // Assert
        Assert.Equal(new Piece(PieceType.King, PieceColor.White), board[0, 4]);
        Assert.Equal(new Piece(PieceType.Queen, PieceColor.Black), board[7, 3]);
    }

    [Theory]
    [InlineData(Board.StartingFen, PieceColor.White)]
    [InlineData("rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1", PieceColor.Black)]
    public void FromFen_ReadsSideToMove(string fen, PieceColor expected)
    {
        var board = Board.FromFen(fen);

        Assert.Equal(expected, board.SideToMove);
    }

    [Theory]
    [InlineData("KQkq", CastlingRights.All)]
    [InlineData("Kq", CastlingRights.WhiteKingside | CastlingRights.BlackQueenside)]
    [InlineData("-", CastlingRights.None)]
    public void FromFen_ReadsCastlingRights(string field, CastlingRights expected)
    {
        var board = Board.FromFen($"r3k2r/8/8/8/8/8/8/R3K2R w {field} - 0 1");

        Assert.Equal(expected, board.CastlingRights);
    }

    [Theory]
    [InlineData(Board.StartingFen)]
    [InlineData("rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1")]
    [InlineData("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1")]
    [InlineData("8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1")]
    [InlineData("r3k2r/8/8/8/8/8/8/R3K2R b Kq - 0 1")]
    public void ToFen_RoundTripsFromFen(string fen)
    {
        Assert.Equal(fen, Board.FromFen(fen).ToFen());
    }

    [Fact]
    public void ToFen_AfterMoves_DescribesNewPosition()
    {
        var board = Board.FromFen(Board.StartingFen);

        board.MakeMove(Move.FromUci("e2e4"));
        board.MakeMove(Move.FromUci("e7e5"));
        board.MakeMove(Move.FromUci("e1e2"));

        // The king move cleared white's castling rights and the en passant square from e7-e5.
        Assert.Equal("rnbqkbnr/pppp1ppp/8/4p3/4P3/8/PPPPKPPP/RNBQ1BNR b kq - 0 1", board.ToFen());
    }

    [Fact]
    public void FromFen_ReadsEnPassantSquare()
    {
        var board = Board.FromFen("rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1");

        Assert.Equal(new Square(2, 4), board.EnPassantSquare);
    }

    [Fact]
    public void FromFen_NoEnPassantSquare_IsNull()
    {
        var board = Board.FromFen(Board.StartingFen);

        Assert.Null(board.EnPassantSquare);
    }

    [Fact]
    public void Clone_MoveOnCopy_DoesNotChangeOriginal()
    {
        // Arrange
        var original = Board.FromFen(Board.StartingFen);
        var copy = original.Clone();
        var e2 = new Square(1, 4);
        var e4 = new Square(3, 4);

        // Act
        copy.MakeMove(new Move(e2, e4));

        // Assert
        Assert.Equal(new Piece(PieceType.Pawn, PieceColor.White), original[e2]);
        Assert.Null(original[e4]);
        Assert.Equal(PieceColor.White, original.SideToMove);
    }

    [Fact]
    public void MakeMove_PawnPush_MovesPieceAndPassesTurn()
    {
        // Arrange
        var board = Board.FromFen(Board.StartingFen);
        var e2 = new Square(1, 4);
        var e4 = new Square(3, 4);

        // Act
        board.MakeMove(new Move(e2, e4));

        // Assert
        Assert.Equal(new Piece(PieceType.Pawn, PieceColor.White), board[e4]);
        Assert.Null(board[e2]);
        Assert.Equal(PieceColor.Black, board.SideToMove);
    }

    [Theory]
    [InlineData("4k3/4p3/8/8/8/8/4P3/4K3 w - - 0 1", 1, 3, 2)] // white e2-e4 skips e3
    [InlineData("4k3/4p3/8/8/8/8/4P3/4K3 b - - 0 1", 6, 4, 5)] // black e7-e5 skips e6
    public void MakeMove_DoublePawnPush_SetsEnPassantSquare(string fen, int fromRank, int toRank, int skippedRank)
    {
        // Arrange
        var board = Board.FromFen(fen);

        // Act
        board.MakeMove(new Move(new Square(fromRank, 4), new Square(toRank, 4)));

        // Assert
        Assert.Equal(new Square(skippedRank, 4), board.EnPassantSquare);
    }

    [Fact]
    public void MakeMove_AfterAnyOtherMove_ClearsEnPassantSquare()
    {
        // Arrange
        var board = Board.FromFen("rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1");

        // Act
        board.MakeMove(new Move(new Square(7, 6), new Square(5, 5))); // Ng8-f6

        // Assert
        Assert.Null(board.EnPassantSquare);
    }

    [Fact]
    public void MakeMove_EnPassant_RemovesCapturedPawn()
    {
        // Arrange: black just played d7-d5 next to the white pawn on e5.
        var board = Board.FromFen("4k3/8/8/3pP3/8/8/8/4K3 w - d6 0 1");
        var e5 = new Square(4, 4);
        var d6 = new Square(5, 3);
        var d5 = new Square(4, 3);

        // Act
        board.MakeMove(new Move(e5, d6));

        // Assert
        Assert.Equal(new Piece(PieceType.Pawn, PieceColor.White), board[d6]);
        Assert.Null(board[e5]);
        Assert.Null(board[d5]);
    }

    [Theory]
    [InlineData(6, 7, 5)] // kingside: e1-g1, rook h1-f1
    [InlineData(2, 0, 3)] // queenside: e1-c1, rook a1-d1
    public void MakeMove_Castling_MovesRookOverKing(int kingToFile, int rookFromFile, int rookToFile)
    {
        // Arrange
        var board = Board.FromFen("r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 0 1");

        // Act
        board.MakeMove(new Move(new Square(0, 4), new Square(0, kingToFile)));

        // Assert
        Assert.Equal(new Piece(PieceType.King, PieceColor.White), board[0, kingToFile]);
        Assert.Equal(new Piece(PieceType.Rook, PieceColor.White), board[0, rookToFile]);
        Assert.Null(board[0, rookFromFile]);
        Assert.Null(board[0, 4]);
    }

    [Fact]
    public void MakeMove_KingMoves_LosesBothRightsForThatColor()
    {
        var board = Board.FromFen("r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 0 1");

        board.MakeMove(new Move(new Square(0, 4), new Square(1, 4)));

        Assert.Equal(CastlingRights.BlackKingside | CastlingRights.BlackQueenside, board.CastlingRights);
    }

    [Fact]
    public void MakeMove_RookLeavesCorner_LosesThatSideOnly()
    {
        var board = Board.FromFen("r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 0 1");

        board.MakeMove(new Move(new Square(0, 7), new Square(3, 7))); // Rh1-h4

        Assert.Equal(CastlingRights.All & ~CastlingRights.WhiteKingside, board.CastlingRights);
    }

    [Fact]
    public void MakeMove_RookCapturedInCorner_LosesThatSide()
    {
        var board = Board.FromFen("r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 0 1");

        board.MakeMove(new Move(new Square(0, 0), new Square(7, 0))); // Ra1xa8

        Assert.Equal(CastlingRights.WhiteKingside | CastlingRights.BlackKingside, board.CastlingRights);
    }

    [Fact]
    public void MakeMove_Capture_ReplacesCapturedPiece()
    {
        // Arrange
        var board = Board.FromFen("8/8/8/3p4/4P3/8/8/8 w - - 0 1");
        var e4 = new Square(3, 4);
        var d5 = new Square(4, 3);

        // Act
        board.MakeMove(new Move(e4, d5));

        // Assert
        Assert.Equal(new Piece(PieceType.Pawn, PieceColor.White), board[d5]);
        Assert.Null(board[e4]);
    }

    [Fact]
    public void MakeMove_Promotion_PlacesNewPieceOfSameColor()
    {
        // Arrange
        var board = Board.FromFen("8/8/8/8/8/8/4p3/8 b - - 0 1");
        var e2 = new Square(1, 4);
        var e1 = new Square(0, 4);

        // Act
        board.MakeMove(new Move(e2, e1, PieceType.Queen));

        // Assert
        Assert.Equal(new Piece(PieceType.Queen, PieceColor.Black), board[e1]);
        Assert.Null(board[e2]);
        Assert.Equal(PieceColor.White, board.SideToMove);
    }
}
