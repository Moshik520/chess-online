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
}
