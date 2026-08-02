namespace SnipText.Core;

public sealed class ScreenRegionCaptureService
{
    private readonly IScreenRegionCaptureBackend _backend;

    public ScreenRegionCaptureService(IScreenRegionCaptureBackend backend)
    {
        _backend = backend ?? throw new ArgumentNullException(nameof(backend));
    }

    public CapturedScreenImage Capture(ScreenSelectionBounds bounds)
    {
        if (bounds.IsEmpty)
        {
            throw new ArgumentException("Capture bounds must be non-empty.", nameof(bounds));
        }

        return _backend.Capture(bounds);
    }
}
