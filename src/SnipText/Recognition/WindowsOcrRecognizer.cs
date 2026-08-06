using SnipText.Capture;
using SnipText.Core;
using Windows.Globalization;
using Windows.Media.Ocr;

namespace SnipText.Recognition;

public sealed class WindowsOcrRecognizer : ITextRecognizer, IConfidenceTextRecognizer
{
    private const string MissingLanguagePackMessage =
        "No Windows OCR language pack is installed. Add one in Settings > Time & language > Language & region, then install OCR components for that language.";

    private readonly OcrEngine? _ocrEngine;

    public WindowsOcrRecognizer(string? preferredLanguageTag = null)
    {
        _ocrEngine = CreateEngine(preferredLanguageTag);
    }

    public async Task<string> RecognizeAsync(CapturedScreenImage image, CancellationToken cancellationToken = default)
    {
        var result = await RecognizeWithConfidenceAsync(image, cancellationToken);
        return result.Text;
    }

    public async Task<TextRecognitionResult> RecognizeWithConfidenceAsync(
        CapturedScreenImage image,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(image);

        if (_ocrEngine is null)
        {
            throw new InvalidOperationException(MissingLanguagePackMessage);
        }

        cancellationToken.ThrowIfCancellationRequested();

        using var softwareBitmap = SoftwareBitmapConversion.ToSoftwareBitmap(image);
        var ocrResult = await _ocrEngine.RecognizeAsync(softwareBitmap).AsTask(cancellationToken);

        var lines = ocrResult.Lines.Select(static line => line.Words.Select(static word => word.Text));
        var text = OcrTextLayoutFormatter.JoinLines(lines);

        return new TextRecognitionResult(text, EstimateConfidence(text));
    }

    private static double EstimateConfidence(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return 0d;
        }

        var visibleCharacters = text.Count(static character => !char.IsWhiteSpace(character));
        if (visibleCharacters == 0)
        {
            return 0d;
        }

        var alphaNumericCharacters = text.Count(char.IsLetterOrDigit);
        var alphaNumericRatio = alphaNumericCharacters / (double)visibleCharacters;
        var lengthFactor = Math.Clamp(visibleCharacters / 32d, 0d, 1d);

        return Math.Clamp((alphaNumericRatio * 0.7d) + (lengthFactor * 0.3d), 0d, 1d);
    }

    private static OcrEngine? CreateEngine(string? preferredLanguageTag)
    {
        if (!string.IsNullOrWhiteSpace(preferredLanguageTag))
        {
            try
            {
                var language = new Language(preferredLanguageTag.Trim());
                if (OcrEngine.IsLanguageSupported(language))
                {
                    return OcrEngine.TryCreateFromLanguage(language);
                }
            }
            catch (Exception)
            {
                // Ignore invalid language tags and fall back to user profile languages.
            }
        }

        return OcrEngine.TryCreateFromUserProfileLanguages();
    }
}
