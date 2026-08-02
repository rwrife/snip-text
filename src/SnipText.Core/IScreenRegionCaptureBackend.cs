namespace SnipText.Core;

public interface IScreenRegionCaptureBackend
{
    CapturedScreenImage Capture(ScreenSelectionBounds bounds);
}
