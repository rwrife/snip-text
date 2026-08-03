using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using SnipText.Core;

namespace SnipText.Capture;

public sealed class SystemDrawingScreenRegionCaptureBackend : IScreenRegionCaptureBackend
{
    private const string DebugCaptureEnvironmentVariable = "SNIPTEXT_DEBUG_SAVE_CAPTURE";

    public CapturedScreenImage Capture(ScreenSelectionBounds bounds)
    {
        if (bounds.IsEmpty)
        {
            throw new ArgumentException("Capture bounds must be non-empty.", nameof(bounds));
        }

        using var bitmap = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);

        using (var graphics = Graphics.FromImage(bitmap))
        {
            graphics.CopyFromScreen(
                sourceX: bounds.X,
                sourceY: bounds.Y,
                destinationX: 0,
                destinationY: 0,
                blockRegionSize: new Size(bounds.Width, bounds.Height),
                copyPixelOperation: CopyPixelOperation.SourceCopy);
        }

        TrySaveDebugCapture(bitmap, bounds);
        var pixels = CopyPixels(bitmap, bounds);
        return new CapturedScreenImage(bounds, pixels);
    }

    private static void TrySaveDebugCapture(Bitmap bitmap, ScreenSelectionBounds bounds)
    {
        var shouldSave = string.Equals(
            Environment.GetEnvironmentVariable(DebugCaptureEnvironmentVariable),
            "1",
            StringComparison.OrdinalIgnoreCase)
            || string.Equals(
                Environment.GetEnvironmentVariable(DebugCaptureEnvironmentVariable),
                "true",
                StringComparison.OrdinalIgnoreCase);

        if (!shouldSave)
        {
            return;
        }

        var folder = Path.Combine(Path.GetTempPath(), "snip-text", "captures");
        Directory.CreateDirectory(folder);

        var fileName = $"capture-{DateTime.UtcNow:yyyyMMdd-HHmmssfff}-x{bounds.X}-y{bounds.Y}-w{bounds.Width}-h{bounds.Height}.png";
        var outputPath = Path.Combine(folder, fileName);
        bitmap.Save(outputPath, ImageFormat.Png);
    }

    private static byte[] CopyPixels(Bitmap bitmap, ScreenSelectionBounds bounds)
    {
        var rect = new Rectangle(0, 0, bounds.Width, bounds.Height);
        var bitmapData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

        try
        {
            var rowLength = checked(bounds.Width * 4);
            var destination = new byte[checked(rowLength * bounds.Height)];

            for (var row = 0; row < bounds.Height; row++)
            {
                var source = IntPtr.Add(bitmapData.Scan0, row * bitmapData.Stride);
                Marshal.Copy(source, destination, row * rowLength, rowLength);
            }

            return destination;
        }
        finally
        {
            bitmap.UnlockBits(bitmapData);
        }
    }
}
