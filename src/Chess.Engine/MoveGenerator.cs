namespace Chess.Engine;

public static class MoveGenerator
{
    private static readonly (int Rank, int File)[] KnightOffsets =
    [
        (2, 1), (2, -1), (-2, 1), (-2, -1),
        (1, 2), (1, -2), (-1, 2), (-1, -2)
    ];

    private static readonly (int Rank, int File)[] RookDirections =
    [
        (1, 0), (-1, 0), (0, 1), (0, -1)
    ];

    private static readonly (int Rank, int File)[] BishopDirections =
    [
        (1, 1), (-1, -1), (1, -1), (-1, 1)
    ];

    private static readonly (int Rank, int File)[] QueenDirections =
        [.. RookDirections, .. BishopDirections];

    private static readonly int[] PawnCaptureFiles = [-1, 1];

    private static readonly PieceType[] PromotionTypes =
        [PieceType.Queen, PieceType.Rook, PieceType.Bishop, PieceType.Knight];

    public static List<Move> GetKnightMoves(Board board, Square from) =>
        GetStepMoves(board, from, KnightOffsets);

    // The king steps one square in any of the queen's directions.
    // Castling needs castling rights, which the board does not track yet.
    public static List<Move> GetKingMoves(Board board, Square from) =>
        GetStepMoves(board, from, QueenDirections);

    public static List<Move> GetRookMoves(Board board, Square from) =>
        GetSlidingMoves(board, from, RookDirections);

    public static List<Move> GetBishopMoves(Board board, Square from) =>
        GetSlidingMoves(board, from, BishopDirections);

    public static List<Move> GetQueenMoves(Board board, Square from) =>
        GetSlidingMoves(board, from, QueenDirections);

    // En passant needs the en passant square, which the board does not track yet.
    public static List<Move> GetPawnMoves(Board board, Square from)
    {
        PieceColor myColor = ColorAt(board, from);
        int forward = myColor == PieceColor.White ? 1 : -1;
        int startRank = myColor == PieceColor.White ? 1 : 6;
        var moves = new List<Move>();

        Square oneStep = new Square(from.Rank + forward, from.File);
        if (oneStep.IsOnBoard && board[oneStep] is null)
        {
            AddPawnMove(moves, from, oneStep);

            Square twoSteps = new Square(from.Rank + 2 * forward, from.File);
            if (from.Rank == startRank && board[twoSteps] is null)
            {
                moves.Add(new Move(from, twoSteps));
            }
        }

        foreach (int fileOffset in PawnCaptureFiles)
        {
            Square target = new Square(from.Rank + forward, from.File + fileOffset);

            if (!target.IsOnBoard)
            {
                continue;
            }

            Piece? targetPiece = board[target];
            if (targetPiece is not null && targetPiece.Value.Color != myColor)
            {
                AddPawnMove(moves, from, target);
            }
        }

        return moves;
    }

    // A pawn that reaches the last rank must promote, so it gets one move per promotion piece.
    private static void AddPawnMove(List<Move> moves, Square from, Square to)
    {
        bool isLastRank = to.Rank == 0 || to.Rank == 7;
        if (!isLastRank)
        {
            moves.Add(new Move(from, to));
            return;
        }

        foreach (PieceType type in PromotionTypes)
        {
            moves.Add(new Move(from, to, type));
        }
    }

    private static Square FindKing(Board board, PieceColor color)
    {
        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 0; file < 8; file++)
            {
                if (board[rank, file] == new Piece(PieceType.King, color))

                {
                    return new Square(rank, file);
                }
            }
        }

        throw new InvalidOperationException($"No king found for color {color}");

    }


    private static PieceColor ColorAt(Board board, Square from) =>
        board[from]?.Color ?? throw new ArgumentException($"No piece on {from}", nameof(from));

    private static List<Move> GetStepMoves(Board board, Square from, (int Rank, int File)[] offsets)
    {
        PieceColor myColor = ColorAt(board, from);
        var moves = new List<Move>();

        foreach (var offset in offsets)
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

    private static List<Move> GetSlidingMoves(Board board, Square from, (int Rank, int File)[] directions)
    {
        PieceColor myColor = ColorAt(board, from);
        var moves = new List<Move>();

        foreach (var direction in directions)
        {
            Square target = from;

            while (true)
            {
                target = new Square(target.Rank + direction.Rank, target.File + direction.File);

                if (!target.IsOnBoard)
                {
                    break;
                }

                Piece? targetPiece = board[target];
                if (targetPiece is null)
                {
                    moves.Add(new Move(from, target));
                    continue;
                }

                if (targetPiece.Value.Color != myColor)
                {
                    moves.Add(new Move(from, target));
                }

                break;
            }
        }

        return moves;
    }

    public static List<Move> GetMoves(Board board, Square from)
    {
        PieceType type = board[from]?.Type
            ?? throw new ArgumentException($"No piece on {from}", nameof(from));

        return type switch
        {
            PieceType.Pawn => GetPawnMoves(board, from),
            PieceType.Knight => GetKnightMoves(board, from),
            PieceType.Bishop => GetBishopMoves(board, from),
            PieceType.Rook => GetRookMoves(board, from),
            PieceType.Queen => GetQueenMoves(board, from),
            PieceType.King => GetKingMoves(board, from),
            _ => throw new ArgumentOutOfRangeException(nameof(from), $"Unknown piece type {type}")
        };
    }

    public static bool IsInCheck(Board board, PieceColor color)
    {
        Square kingSquare = FindKing(board, color);
        

        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 0; file < 8; file++)
            {
                Piece? piece = board[rank, file];
                if (piece is null || piece.Value.Color == color)
                {
                    continue;
                }

                var moves = GetMoves(board, new Square(rank, file));

                if (moves.Any(move => move.To == kingSquare))
                {
                    return true;
                }

            }
        }
        return false;

    }





}
