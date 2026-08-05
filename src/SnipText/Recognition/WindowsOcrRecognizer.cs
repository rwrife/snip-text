using SnipText.Capture;
using SnipText.Core;
using Windows.Globalization;
using Windows.Media.Ocr;

namespace SnipText.Recognition;

public sealed class WindowsOcrRecognizer : ITextRecognizer
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
        ArgumentNullException.ThrowIfNull(image);

        if (_ocrEngine is null)
        {
            throw new InvalidOperationException(MissingLanguagePackMessage);
        }

        cancellationToken.ThrowIfCancellationRequested();

        using var softwareBitmap = SoftwareBitmapConversion.ToSoftwareBitmap(image);
        var ocrResult = await _ocrEngine.RecognizeAsync(softwareBitmap).AsTask(cancellationToken);

        var lines = ocrResult.Lines.Select(static line => line.Words.Select(static word => word.Text));
        return OcrTextLayoutFormatter.JoinLines(lines);
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
