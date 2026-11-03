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

    [Theory]
    [InlineData(0, 0, "a1")]
    [InlineData(2, 4, "e3")]
    [InlineData(7, 7, "h8")]
    public void ToString_ReturnsAlgebraicName(int rank, int file, string expected)
    {
        Assert.Equal(expected, new Square(rank, file).ToString());
    }

    [Theory]
    [InlineData("a1", 0, 0)]
    [InlineData("e3", 2, 4)]
    [InlineData("h8", 7, 7)]
    public void Parse_ReadsAlgebraicName(string text, int rank, int file)
    {
        Assert.Equal(new Square(rank, file), Square.Parse(text));
    }
}
