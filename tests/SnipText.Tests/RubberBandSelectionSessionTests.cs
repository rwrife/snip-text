using SnipText.Core;
using Xunit;

namespace SnipText.Tests;

public class RubberBandSelectionSessionTests
{
    [Fact]
    public void FromPoints_NormalizesToTopLeftAndPositiveSize()
    {
        var bounds = ScreenSelectionBounds.FromPoints(300, 220, 120, 80);

        Assert.Equal(120, bounds.X);
        Assert.Equal(80, bounds.Y);
        Assert.Equal(180, bounds.Width);
        Assert.Equal(140, bounds.Height);
    }

    [Fact]
    public void Complete_ReturnsCurrentSelectionAndResetsState()
    {
        var session = new RubberBandSelectionSession();
        session.Begin(10, 12);
        session.Update(24, 40);

        var completed = session.Complete(40, 52);

        Assert.Equal(new ScreenSelectionBounds(10, 12, 30, 40), completed);
        Assert.False(session.IsSelecting);
        Assert.Equal(default, session.CurrentBounds);
    }

    [Fact]
    public void Cancel_ClearsCurrentSelection()
    {
        var session = new RubberBandSelectionSession();
        session.Begin(4, 6);
        session.Update(10, 14);

        session.Cancel();

        Assert.False(session.IsSelecting);
        Assert.Equal(default, session.CurrentBounds);
    }

    [Fact]
    public void Update_ThrowsWhenBeginWasNotCalled()
    {
        var session = new RubberBandSelectionSession();

        Assert.Throws<InvalidOperationException>(() => session.Update(1, 1));
    }
}
