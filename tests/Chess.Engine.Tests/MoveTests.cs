namespace Chess.Engine.Tests;

public class MoveTests
{
    private static readonly Square E2 = new(1, 4);
    private static readonly Square E4 = new(3, 4);
    private static readonly Square E7 = new(6, 4);
    private static readonly Square E8 = new(7, 4);

    [Fact]
    public void ToUci_NormalMove_IsTwoSquares()
    {
        Assert.Equal("e2e4", new Move(E2, E4).ToUci());
    }

    [Theory]
    [InlineData(PieceType.Queen, "e7e8q")]
    [InlineData(PieceType.Rook, "e7e8r")]
    [InlineData(PieceType.Bishop, "e7e8b")]
    [InlineData(PieceType.Knight, "e7e8n")]
    public void ToUci_Promotion_AppendsPieceLetter(PieceType promotion, string expected)
    {
        Assert.Equal(expected, new Move(E7, E8, promotion).ToUci());
    }

    [Fact]
    public void FromUci_NormalMove_ReadsBothSquares()
    {
        Assert.Equal(new Move(E2, E4), Move.FromUci("e2e4"));
    }

    [Fact]
    public void FromUci_Promotion_ReadsPieceLetter()
    {
        Assert.Equal(new Move(E7, E8, PieceType.Knight), Move.FromUci("e7e8n"));
    }

    [Fact]
    public void FromUci_UnknownPromotionLetter_Throws()
    {
        Assert.Throws<ArgumentException>(() => Move.FromUci("e7e8x"));
    }

    [Fact]
    public void FromUci_IsTheInverseOfToUci_ForEveryLegalMove()
    {
        // Kiwipete has castling, promotions-to-be and en passant chances, so it covers many move shapes.
        var board = Board.FromFen("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1");

        foreach (var move in MoveGenerator.GetLegalMoves(board))
        {
            Assert.Equal(move, Move.FromUci(move.ToUci()));
        }
    }
}
