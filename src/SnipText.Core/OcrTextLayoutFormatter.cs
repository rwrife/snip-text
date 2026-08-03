namespace SnipText.Core;

public static class OcrTextLayoutFormatter
{
    public static string JoinLines(IEnumerable<IEnumerable<string>> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);

        var normalizedLines = new List<string>();
        foreach (var line in lines)
        {
            if (line is null)
            {
                continue;
            }

            var words = line
                .Where(static word => !string.IsNullOrWhiteSpace(word))
                .Select(static word => word.Trim())
                .ToArray();

            if (words.Length == 0)
            {
                continue;
            }

            normalizedLines.Add(string.Join(' ', words));
        }

        return string.Join(Environment.NewLine, normalizedLines);
    }
}
