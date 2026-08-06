namespace SnipText.Core;

public sealed record TextRecognitionResult
{
    public TextRecognitionResult(string? text, double confidence)
    {
        Text = text ?? string.Empty;
        Confidence = double.IsNaN(confidence)
            ? 0d
            : Math.Clamp(confidence, 0d, 1d);
    }

    public string Text { get; }

    public double Confidence { get; }
}
