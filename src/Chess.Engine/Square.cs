namespace Chess.Engine;

public readonly record struct Square(int Rank, int File)
{
    public bool IsOnBoard => Rank >= 0 && Rank <= 7 && File >= 0 && File <= 7;

    public override string ToString() => $"{(char)('a' + File)}{Rank + 1}";

    public static Square Parse(string text)
    {
        int file = text[0] - 'a';
        int rank = text[1] - '1';
        return new Square(rank, file);
    }
}
