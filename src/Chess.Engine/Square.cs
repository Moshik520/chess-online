namespace Chess.Engine;



public readonly record struct Square(int Rank, int File)
{
    public bool IsOnBoard => Rank >= 0 && Rank <= 7 && File >= 0 && File <= 7;
}




