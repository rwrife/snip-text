namespace SnipText.Core;

public interface ITextRecognizer
{
    Task<string> RecognizeAsync(CapturedScreenImage image, CancellationToken cancellationToken = default);
}
