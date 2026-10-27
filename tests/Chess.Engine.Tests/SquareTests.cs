using Chess.Engine;

namespace Chess.Engine.Tests;

public class SquareTests
{
    [Theory]
    [InlineData(0, 0, true)]
    [InlineData(7, 7, true)]
    [InlineData(-1, 0, false)]
    [InlineData(0, -1, false)]
    [InlineData(8, 2, false)]
    [InlineData(2, 8, false)]
    public void IsOnBoard_ReturnsCorrectValue(int rank, int file, bool expected)
    {
        Square square = new Square(rank, file);

        Assert.Equal(expected, square.IsOnBoard);
    }
    
}
