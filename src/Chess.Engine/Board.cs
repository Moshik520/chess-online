
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

        return board;
    }

    public Piece? this[Square square]
    {
        get => this[square.Rank, square.File];
        set => this[square.Rank, square.File] = value;
    }

    public void MakeMove(Move move)
    {


        Piece piece = this[move.From] ?? throw new ArgumentException($"No piece on {move.From}", nameof(move));


        this[move.To] = move.Promotion is not null
            ? new Piece(move.Promotion.Value, piece.Color)
            : piece;
        this[move.From] = null;


        SideToMove = SideToMove == PieceColor.White ? PieceColor.Black : PieceColor.White;

    }



}





