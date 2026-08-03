using System.Runtime.InteropServices.WindowsRuntime;
using SnipText.Core;
using Windows.Graphics.Imaging;

namespace SnipText.Capture;

public static class SoftwareBitmapConversion
{
    public static SoftwareBitmap ToSoftwareBitmap(CapturedScreenImage image)
    {
        ArgumentNullException.ThrowIfNull(image);

        var softwareBitmap = new SoftwareBitmap(
            BitmapPixelFormat.Bgra8,
            image.Width,
            image.Height,
            BitmapAlphaMode.Premultiplied);

        softwareBitmap.CopyFromBuffer(image.Bgra32Pixels.AsBuffer());
        return softwareBitmap;
    }
}
