namespace SnipText.Core;

public sealed class CapturedScreenImage
{
    public CapturedScreenImage(ScreenSelectionBounds bounds, byte[] bgra32Pixels)
    {
        if (bounds.IsEmpty)
        {
            throw new ArgumentException("Capture bounds must be non-empty.", nameof(bounds));
        }

        ArgumentNullException.ThrowIfNull(bgra32Pixels);

        var stride = checked(bounds.Width * 4);
        var expectedLength = checked(stride * bounds.Height);
        if (bgra32Pixels.Length != expectedLength)
        {
            throw new ArgumentException(
                $"Pixel buffer length {bgra32Pixels.Length} does not match expected BGRA32 size {expectedLength} for {bounds}.",
                nameof(bgra32Pixels));
        }

        Bounds = bounds;
        Stride = stride;
        Bgra32Pixels = bgra32Pixels;
    }

    public ScreenSelectionBounds Bounds { get; }

    public int Width => Bounds.Width;

    public int Height => Bounds.Height;

    public int Stride { get; }

    public byte[] Bgra32Pixels { get; }
}
