
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


    public Piece? this[int rank, int file]
    {
        get => _squares[rank, file];
        set => _squares[rank, file] = value;
    }

    public static Board FromFen(string fen)
    {
        Board board = new Board();
        string first = fen.Split(' ')[0];
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
        return board;
    }

}





