namespace Chess.Engine;

public static class MoveGenerator
{
    private static readonly (int Rank, int File)[] KnightOffsets =
[
    (2, 1), (2, -1), (-2, 1), (-2, -1),
    (1, 2), (1, -2), (-1, 2), (-1, -2)
];


    public static List<Move> GetKnightMoves(Board board, Square from)
    {
        PieceColor myColor = board[from].Value.Color;
        var moves = new List<Move>();

        foreach (var offset in KnightOffsets)
        {
            Square target = new Square(from.Rank + offset.Rank, from.File + offset.File);

            if (!target.IsOnBoard)
            {
                continue;
            }


            Piece? targetPiece = board[target];
            if (targetPiece is not null && targetPiece.Value.Color == myColor)
            {
                continue;
            }

            moves.Add(new Move(from, target));

        }

        return moves;
    }
}
