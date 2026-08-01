using SnipText.Core;
using System.Windows.Forms;

namespace SnipText.Capture;

public sealed class CaptureOverlayService
{
    public ScreenSelectionBounds? ShowAndSelect()
    {
        var virtualScreen = SystemInformation.VirtualScreen;
        var overlayWindow = new CaptureOverlayWindow();
        overlayWindow.ConfigureBounds(virtualScreen);

        var accepted = overlayWindow.ShowDialog();
        return accepted == true ? overlayWindow.SelectedBounds : null;
    }
}
