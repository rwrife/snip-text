namespace SnipText.Core;

public interface IConfidenceTextRecognizer
{
    Task<TextRecognitionResult> RecognizeWithConfidenceAsync(
        CapturedScreenImage image,
        CancellationToken cancellationToken = default);
}
