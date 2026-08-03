using SnipText.Core;
using Xunit;

namespace SnipText.Tests;

public sealed class ScreenRegionCaptureServiceTests
{
    [Fact]
    public void Capture_ForwardsExactBoundsIncludingNegativeCoordinates()
    {
        var bounds = new ScreenSelectionBounds(-1920, -150, 640, 320);
        var backend = new RecordingBackend();
        var service = new ScreenRegionCaptureService(backend);

        var image = service.Capture(bounds);

        Assert.Equal(bounds, backend.LastBounds);
        Assert.Equal(bounds, image.Bounds);
        Assert.Equal(bounds.Width, image.Width);
        Assert.Equal(bounds.Height, image.Height);
        Assert.Equal(bounds.Width * 4 * bounds.Height, image.Bgra32Pixels.Length);
    }

    [Fact]
    public void Capture_ThrowsForEmptyBounds()
    {
        var service = new ScreenRegionCaptureService(new RecordingBackend());

        Assert.Throws<ArgumentException>(() => service.Capture(default));
    }

    [Fact]
    public void CapturedScreenImage_ThrowsWhenPixelBufferHasUnexpectedLength()
    {
        var bounds = new ScreenSelectionBounds(10, 20, 3, 2);

        var ex = Assert.Throws<ArgumentException>(() => new CapturedScreenImage(bounds, new byte[10]));

        Assert.Contains("Pixel buffer length", ex.Message);
    }

    private sealed class RecordingBackend : IScreenRegionCaptureBackend
    {
        public ScreenSelectionBounds LastBounds { get; private set; }

        public CapturedScreenImage Capture(ScreenSelectionBounds bounds)
        {
            LastBounds = bounds;
            return new CapturedScreenImage(bounds, new byte[bounds.Width * bounds.Height * 4]);
        }
    }
}
