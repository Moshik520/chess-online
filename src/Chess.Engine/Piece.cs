namespace Chess.Engine;

public enum PieceColor
{
    White, Black
}

public enum PieceType
{
    Pawn, Knight, Bishop, Rook, Queen, King
}

public readonly record struct Piece(PieceType Type, PieceColor Color);
