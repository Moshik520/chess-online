using System.Drawing;

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
            if ((targetPiece is not null && targetPiece.Value.Color != myColor) || target == board.EnPassantSquare)
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

    public static bool IsInCheck(Board board, PieceColor color) =>
        IsSquareAttacked(board, FindKing(board, color), Opponent(color));

    public static bool IsSquareAttacked(Board board, Square square, PieceColor byColor)
    {
        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 0; file < 8; file++)
            {
                Piece? piece = board[rank, file];
                if (piece is null || piece.Value.Color != byColor)
                {
                    continue;
                }

                // A pawn attacks diagonally even when the square is empty, and its forward
                // push never attacks, so its move list cannot answer this question.
                if (piece.Value.Type == PieceType.Pawn)
                {
                    int forward = byColor == PieceColor.White ? 1 : -1;
                    if (square.Rank == rank + forward && Math.Abs(square.File - file) == 1)
                    {
                        return true;
                    }

                    continue;
                }

                var moves = GetMoves(board, new Square(rank, file));

                if (moves.Any(move => move.To == square))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static PieceColor Opponent(PieceColor color) =>
        color == PieceColor.White ? PieceColor.Black : PieceColor.White;

    // Castling is generated here and not in GetKingMoves: it needs attack checks, and
    // IsSquareAttacked calls GetKingMoves, so putting it there would recurse forever.
    private static void AddCastlingMoves(Board board, List<Move> moves, PieceColor us)
    {
        int rank = us == PieceColor.White ? 0 : 7;
        var kingSquare = new Square(rank, 4);
        var king = new Piece(PieceType.King, us);
        var rook = new Piece(PieceType.Rook, us);
        PieceColor them = Opponent(us);

        if (board[kingSquare] != king || IsSquareAttacked(board, kingSquare, them))
        {
            return;
        }

        CastlingRights kingside = us == PieceColor.White ? CastlingRights.WhiteKingside : CastlingRights.BlackKingside;
        if (board.CastlingRights.HasFlag(kingside)
            && board[rank, 7] == rook
            && board[rank, 5] is null && board[rank, 6] is null
            && !IsSquareAttacked(board, new Square(rank, 5), them)
            && !IsSquareAttacked(board, new Square(rank, 6), them))
        {
            moves.Add(new Move(kingSquare, new Square(rank, 6)));
        }

        // On the queenside the rook also passes b1/b8, so it must be empty but may be attacked.
        CastlingRights queenside = us == PieceColor.White ? CastlingRights.WhiteQueenside : CastlingRights.BlackQueenside;
        if (board.CastlingRights.HasFlag(queenside)
            && board[rank, 0] == rook
            && board[rank, 1] is null && board[rank, 2] is null && board[rank, 3] is null
            && !IsSquareAttacked(board, new Square(rank, 3), them)
            && !IsSquareAttacked(board, new Square(rank, 2), them))
        {
            moves.Add(new Move(kingSquare, new Square(rank, 2)));
        }
    }

    public static List<Move> GetLegalMoves(Board board)
    {
        var legalMoves = new List<Move>();
        PieceColor us = board.SideToMove;

        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 0; file < 8; file++)
            {
                Piece? piece = board[rank, file];
                if (piece is null || piece.Value.Color != us)
                {
                    continue;
                }

                var moves = GetMoves(board, new Square(rank, file));

                foreach (var move in moves)
                {
                    Board copyBoard = board.Clone();
                    copyBoard.MakeMove(move);

                    if (!IsInCheck(copyBoard, us))
                    {
                        legalMoves.Add(move);
                    }
                }
            }
        }

        AddCastlingMoves(board, legalMoves, us);

        return legalMoves;
    }

    // Counts the positions reachable in exactly `depth` moves. Comparing the counts with
    // published values validates the whole move generator, including the rare rules.
    public static long Perft(Board board, int depth)
    {
        if (depth == 0)
        {
            return 1;
        }

        var moves = GetLegalMoves(board);
        if (depth == 1)
        {
            return moves.Count;
        }

        long count = 0;
        foreach (var move in moves)
        {
            Board next = board.Clone();
            next.MakeMove(move);
            count += Perft(next, depth - 1);
        }

        return count;
    }

    public static GameStatus GetGameStatus(Board board)
    {
        if (GetLegalMoves(board).Count != 0)
        {
            return GameStatus.InProgress;
        }
        if (IsInCheck(board, board.SideToMove))
        {
            return GameStatus.Checkmate;
        }
        return GameStatus.Stalemate;
    }
}
