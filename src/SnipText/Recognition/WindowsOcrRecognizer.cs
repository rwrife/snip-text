using SnipText.Capture;
using SnipText.Core;
using Windows.Media.Ocr;

namespace SnipText.Recognition;

public sealed class WindowsOcrRecognizer : ITextRecognizer
{
    private const string MissingLanguagePackMessage =
        "No Windows OCR language pack is installed. Add one in Settings > Time & language > Language & region, then install OCR components for that language.";

    private readonly OcrEngine? _ocrEngine;

    public WindowsOcrRecognizer()
    {
        _ocrEngine = OcrEngine.TryCreateFromUserProfileLanguages();
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
}
