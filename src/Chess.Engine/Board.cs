namespace Chess.Engine;

public class Board
{
    private readonly Piece?[,] _squares = new Piece?[8, 8];

    public Piece? this[int rank, int file]
    {
        get => _squares[rank, file];
        set => _squares[rank, file] = value;
    }
}





