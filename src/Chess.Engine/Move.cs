
namespace Chess.Engine;

public readonly record struct Move(Square From, Square To, PieceType? Promotion = null)
{
    public string ToUci()
    {
        if(Promotion is null)
        {
            return $"{From.ToString()}{To.ToString()}";
        }
        char type = Promotion.Value switch
        {
            PieceType.Knight => 'n',
            PieceType.Bishop => 'b' ,
            PieceType.Rook => 'r',
            PieceType.Queen => 'q',
            _ => throw new InvalidOperationException($"Unknown type promotion piece {Promotion}")
        };
        return $"{From.ToString()}{To.ToString()}{type}";
    }

    public static Move FromUci(string text)
    {
        Square from = Square.Parse(text[..2]);
        Square to = Square.Parse(text[2..4]);

        if (text.Length == 4)
        {
            return new Move(from, to);
        }

        PieceType type = text[4] switch
        {
           
                'n' => PieceType.Knight,
                'b' => PieceType.Bishop,
                'r' => PieceType.Rook,
                'q' => PieceType.Queen,
            _ => throw new ArgumentException($"Unknown promotion piece '{text[4]}'", nameof(text))
        };

        return new Move(from, to, type);
    }


}
