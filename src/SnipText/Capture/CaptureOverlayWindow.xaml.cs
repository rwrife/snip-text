using SnipText.Core;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace SnipText.Capture;

public partial class CaptureOverlayWindow : Window
{
    private readonly RubberBandSelectionSession _selectionSession = new();

    public CaptureOverlayWindow()
    {
        InitializeComponent();

        Loaded += (_, _) =>
        {
            Focus();
            Keyboard.Focus(this);
        };

        PreviewKeyDown += OnPreviewKeyDown;
        MouseLeftButtonDown += OnMouseLeftButtonDown;
        MouseMove += OnMouseMove;
        MouseLeftButtonUp += OnMouseLeftButtonUp;
    }

    public ScreenSelectionBounds? SelectedBounds { get; private set; }

    public void ConfigureBounds(System.Drawing.Rectangle virtualScreen)
    {
        Left = virtualScreen.Left;
        Top = virtualScreen.Top;
        Width = virtualScreen.Width;
        Height = virtualScreen.Height;
    }

    private void OnPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key != Key.Escape)
        {
            return;
        }

        e.Handled = true;
        SelectedBounds = null;
        Close();
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var screenPoint = PointToScreen(e.GetPosition(this));
        _selectionSession.Begin(ToScreenPixel(screenPoint.X), ToScreenPixel(screenPoint.Y));

        SelectionRectangle.Visibility = Visibility.Visible;
        CaptureMouse();

        UpdateSelectionVisual(_selectionSession.CurrentBounds);
    }

    private void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (!_selectionSession.IsSelecting)
        {
            return;
        }

        var screenPoint = PointToScreen(e.GetPosition(this));
        var bounds = _selectionSession.Update(ToScreenPixel(screenPoint.X), ToScreenPixel(screenPoint.Y));
        UpdateSelectionVisual(bounds);
    }

    private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_selectionSession.IsSelecting)
        {
            return;
        }

        var screenPoint = PointToScreen(e.GetPosition(this));
        var bounds = _selectionSession.Complete(ToScreenPixel(screenPoint.X), ToScreenPixel(screenPoint.Y));

        if (IsCaptured(bounds))
        {
            SelectedBounds = bounds;
            DialogResult = true;
        }
        else
        {
            SelectedBounds = null;
            DialogResult = false;
        }

        ReleaseMouseCapture();
        Close();
    }

    private void UpdateSelectionVisual(ScreenSelectionBounds bounds)
    {
        var topLeft = PointFromScreen(new System.Windows.Point(bounds.X, bounds.Y));
        var bottomRight = PointFromScreen(new System.Windows.Point(bounds.Right, bounds.Bottom));

        var x = Math.Min(topLeft.X, bottomRight.X);
        var y = Math.Min(topLeft.Y, bottomRight.Y);
        var width = Math.Abs(bottomRight.X - topLeft.X);
        var height = Math.Abs(bottomRight.Y - topLeft.Y);

        Canvas.SetLeft(SelectionRectangle, x);
        Canvas.SetTop(SelectionRectangle, y);
        SelectionRectangle.Width = width;
        SelectionRectangle.Height = height;
    }

    private static int ToScreenPixel(double value) => Convert.ToInt32(Math.Round(value, MidpointRounding.AwayFromZero));

    private static bool IsCaptured(ScreenSelectionBounds bounds) => !bounds.IsEmpty;
}
