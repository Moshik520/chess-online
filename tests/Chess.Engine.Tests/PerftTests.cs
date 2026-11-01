namespace Chess.Engine.Tests;

// Expected counts are the published reference values from
// https://www.chessprogramming.org/Perft_Results
public class PerftTests
{
    private const string Kiwipete = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1";
    private const string Position3 = "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1";
    private const string Position4 = "r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1";
    private const string Position5 = "rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8";

    [Theory]
    [InlineData(Board.StartingFen, 1, 20)]
    [InlineData(Board.StartingFen, 2, 400)]
    [InlineData(Board.StartingFen, 3, 8_902)]
    [InlineData(Board.StartingFen, 4, 197_281)]
    [InlineData(Kiwipete, 1, 48)]           // castling, en passant, promotions, pins
    [InlineData(Kiwipete, 2, 2_039)]
    [InlineData(Kiwipete, 3, 97_862)]
    [InlineData(Position3, 1, 14)]          // en passant that exposes the king
    [InlineData(Position3, 2, 191)]
    [InlineData(Position3, 3, 2_812)]
    [InlineData(Position3, 4, 43_238)]
    [InlineData(Position4, 1, 6)]           // promotions and checks
    [InlineData(Position4, 2, 264)]
    [InlineData(Position4, 3, 9_467)]
    [InlineData(Position5, 1, 44)]
    [InlineData(Position5, 2, 1_486)]
    [InlineData(Position5, 3, 62_379)]
    public void Perft_MatchesPublishedCounts(string fen, int depth, long expected)
    {
        var board = Board.FromFen(fen);

        Assert.Equal(expected, MoveGenerator.Perft(board, depth));
    }
}
