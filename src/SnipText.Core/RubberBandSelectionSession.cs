namespace SnipText.Core;

public sealed class RubberBandSelectionSession
{
    private int? _startX;
    private int? _startY;
    private int _currentX;
    private int _currentY;

    public bool IsSelecting => _startX.HasValue && _startY.HasValue;

    public ScreenSelectionBounds CurrentBounds
    {
        get
        {
            if (!IsSelecting)
            {
                return default;
            }

            return ScreenSelectionBounds.FromPoints(_startX!.Value, _startY!.Value, _currentX, _currentY);
        }
    }

    public void Begin(int x, int y)
    {
        _startX = x;
        _startY = y;
        _currentX = x;
        _currentY = y;
    }

    public ScreenSelectionBounds Update(int x, int y)
    {
        EnsureSelecting();

        _currentX = x;
        _currentY = y;
        return CurrentBounds;
    }

    public ScreenSelectionBounds Complete(int x, int y)
    {
        EnsureSelecting();

        _currentX = x;
        _currentY = y;
        var result = CurrentBounds;
        Cancel();
        return result;
    }

    public void Cancel()
    {
        _startX = null;
        _startY = null;
        _currentX = 0;
        _currentY = 0;
    }

    private void EnsureSelecting()
    {
        if (!IsSelecting)
        {
            throw new InvalidOperationException("Selection has not started.");
        }
    }
}
