
using System.Drawing;

namespace Chess.Engine;

public class Board
{
    private readonly Piece?[,] _squares = new Piece?[8, 8];

    private static Piece PieceFromChar(char c)
    {

        PieceColor color = char.IsLower(c) ? PieceColor.Black : PieceColor.White;

        PieceType type = char.ToLower(c) switch
        {
            'n' => PieceType.Knight,
            'b' => PieceType.Bishop,
            'r' => PieceType.Rook,
            'q' => PieceType.Queen,
            'k' => PieceType.King,
            'p' => PieceType.Pawn,
            _ => throw new ArgumentException($"Unknown FEN piece '{c}'")
        };

        return new Piece(type, color); 
    }

    public const string StartingFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

    public Piece? this[int rank, int file]
    {
        get => _squares[rank, file];
        set => _squares[rank, file] = value;
    }

    public PieceColor SideToMove { get; private set; }

    public CastlingRights CastlingRights { get; private set; }


    public static Board FromFen(string fen)
    {
        Board board = new Board();
        string[] parts = fen.Split(' ');

        string first = parts[0];
        
        int rank = 7;
        int file = 0;


        foreach (char c in first)
        {
            if (c == '/')
            {
                file = 0;
                rank--;
            }
            else if (char.IsDigit(c))
            {
                file += c - '0';
            }
            else
            {
                Piece piece = PieceFromChar(c);
                board._squares[rank, file] = piece;
                file++;
            }
        }

        board.SideToMove = parts[1] == "w" ? PieceColor.White : PieceColor.Black;
        CastlingRights rights = CastlingRights.None;
        foreach (char c in parts[2])
        {
            rights |= c switch
            {
                'K' => CastlingRights.WhiteKingside,
                'Q' => CastlingRights.WhiteQueenside,
                'k' => CastlingRights.BlackKingside,
                'q' => CastlingRights.BlackQueenside,
                '-' => CastlingRights.None,
                _ => throw new ArgumentException($"Unknown castling right '{c}'")
            };
        }
        board.CastlingRights = rights;

        board.EnPassantSquare = parts[3] != "-" ? ParseSquare(parts[3]) : null;


        return board;
    }

    private static Square ParseSquare(string text)
    {
        int file = text[0] - 'a';
        int rank = text[1] - '1';
        return new Square(rank, file);
    }

    public Piece? this[Square square]
    {
        get => this[square.Rank, square.File];
        set => this[square.Rank, square.File] = value;
    }

    public Square? EnPassantSquare { get; private set; }


    public void MakeMove(Move move)
    {
        Piece piece = this[move.From] ?? throw new ArgumentException($"No piece on {move.From}", nameof(move));

        this[move.To] = move.Promotion is not null
            ? new Piece(move.Promotion.Value, piece.Color)
            : piece;
        this[move.From] = null;

        // The pawn taken en passant sits beside the destination: on the mover's rank, in the destination's file.
        bool isEnPassantCapture = piece.Type == PieceType.Pawn && move.To == EnPassantSquare;
        if (isEnPassantCapture)
        {
            this[move.From.Rank, move.To.File] = null;
        }

        // Castling is sent as a two-square king move; the rook jumps to the square the king passed over.
        bool isCastling = piece.Type == PieceType.King
            && Math.Abs(move.To.File - move.From.File) == 2;
        if (isCastling)
        {
            bool isKingside = move.To.File == 6;
            int rookFromFile = isKingside ? 7 : 0;
            int rookToFile = isKingside ? 5 : 3;

            this[move.From.Rank, rookToFile] = this[move.From.Rank, rookFromFile];
            this[move.From.Rank, rookFromFile] = null;
        }

        if (piece.Type == PieceType.King)
        {
            CastlingRights &= piece.Color == PieceColor.White
                ? ~(CastlingRights.WhiteKingside | CastlingRights.WhiteQueenside)
                : ~(CastlingRights.BlackKingside | CastlingRights.BlackQueenside);
        }

        // Moving from a corner means the rook left; moving to a corner means it was captured.
        CastlingRights &= ~CornerRight(move.From) & ~CornerRight(move.To);

        bool isDoublePawnPush = piece.Type == PieceType.Pawn
            && Math.Abs(move.To.Rank - move.From.Rank) == 2;

        EnPassantSquare = isDoublePawnPush
            ? new Square((move.From.Rank + move.To.Rank) / 2, move.From.File)
            : null;

        SideToMove = SideToMove == PieceColor.White ? PieceColor.Black : PieceColor.White;
    }



    private static CastlingRights CornerRight(Square square) => (square.Rank, square.File) switch
    {
        (0, 0) => CastlingRights.WhiteQueenside,
        (0, 7) => CastlingRights.WhiteKingside,
        (7, 0) => CastlingRights.BlackQueenside,
        (7, 7) => CastlingRights.BlackKingside,
        _ => CastlingRights.None
    };

    public Board Clone()
    {

        Board copyBoard = new Board();

        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 0; file < 8; file++)
            {
                copyBoard._squares[rank, file] = _squares[rank, file];


            }
        }
        copyBoard.SideToMove = SideToMove;
        copyBoard.EnPassantSquare = EnPassantSquare;
        copyBoard.CastlingRights = CastlingRights;

        return copyBoard;

    }


}





