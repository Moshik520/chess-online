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
