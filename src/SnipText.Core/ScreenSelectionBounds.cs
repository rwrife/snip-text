namespace SnipText.Core;

public readonly record struct ScreenSelectionBounds(int X, int Y, int Width, int Height)
{
    public bool IsEmpty => Width <= 0 || Height <= 0;

    public int Right => X + Width;

    public int Bottom => Y + Height;

    public static ScreenSelectionBounds FromPoints(int x1, int y1, int x2, int y2)
    {
        var left = Math.Min(x1, x2);
        var top = Math.Min(y1, y2);
        var right = Math.Max(x1, x2);
        var bottom = Math.Max(y1, y2);

        return new ScreenSelectionBounds(left, top, Math.Max(0, right - left), Math.Max(0, bottom - top));
    }

    public override string ToString() => $"X={X}, Y={Y}, Width={Width}, Height={Height}";
}
