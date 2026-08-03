using SnipText.Core;
using Xunit;

namespace SnipText.Tests;

public sealed class OcrTextLayoutFormatterTests
{
    [Fact]
    public void JoinLines_PreservesLineBreaksAndWordSpacing()
    {
        var lines = new[]
        {
            new[] { "Hello", "world" },
            new[] { "Second", "line", "text" },
        };

        var text = OcrTextLayoutFormatter.JoinLines(lines);

        Assert.Equal($"Hello world{Environment.NewLine}Second line text", text);
    }

    [Fact]
    public void JoinLines_TrimsWordsAndSkipsEmptyEntries()
    {
        var lines = new[]
        {
            new[] { "  Alpha ", "", " Beta", "   " },
            new[] { "", "  ", "Gamma" },
        };

        var text = OcrTextLayoutFormatter.JoinLines(lines);

        Assert.Equal($"Alpha Beta{Environment.NewLine}Gamma", text);
    }

    [Fact]
    public void JoinLines_SkipsEmptyLines()
    {
        var lines = new[]
        {
            Array.Empty<string>(),
            new[] { "Single" },
            new[] { "   " },
        };

        var text = OcrTextLayoutFormatter.JoinLines(lines);

        Assert.Equal("Single", text);
    }
}
