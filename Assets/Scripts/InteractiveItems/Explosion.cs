using Mirror;

public struct Explosion
{
    public readonly int Up;
    public readonly int Right;
    public readonly int Down;
    public readonly int Left;

    public Explosion(int up, int right, int down, int left)
    {
        Up = up;
        Right = right;
        Down = down;
        Left = left;
    }
}
