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
}
