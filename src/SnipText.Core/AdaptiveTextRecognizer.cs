using System.Net.Http;

namespace SnipText.Core;

public sealed class AdaptiveTextRecognizer : ITextRecognizer
{
    private readonly ITextRecognizer _nativeRecognizer;
    private readonly ITextRecognizer _localAiRecognizer;
    private readonly LocalAiRoutingMode _routingMode;
    private readonly double _nativeLowConfidenceThreshold;

    public AdaptiveTextRecognizer(
        ITextRecognizer nativeRecognizer,
        ITextRecognizer localAiRecognizer,
        LocalAiRoutingMode routingMode,
        double nativeLowConfidenceThreshold)
    {
        _nativeRecognizer = nativeRecognizer ?? throw new ArgumentNullException(nameof(nativeRecognizer));
        _localAiRecognizer = localAiRecognizer ?? throw new ArgumentNullException(nameof(localAiRecognizer));
        _routingMode = routingMode;
        _nativeLowConfidenceThreshold = Math.Clamp(nativeLowConfidenceThreshold, 0d, 1d);
    }

    public async Task<string> RecognizeAsync(CapturedScreenImage image, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(image);

        return _routingMode switch
        {
            LocalAiRoutingMode.NativeOnly => await _nativeRecognizer.RecognizeAsync(image, cancellationToken),
            LocalAiRoutingMode.AiOnly => await TryLocalAiThenNativeAsync(image, cancellationToken),
            LocalAiRoutingMode.AiFallbackWhenNativeConfidenceLow =>
                await RecognizeWithConfidenceRoutingAsync(image, cancellationToken),
            _ => await _nativeRecognizer.RecognizeAsync(image, cancellationToken),
        };
    }

    private async Task<string> RecognizeWithConfidenceRoutingAsync(CapturedScreenImage image, CancellationToken cancellationToken)
    {
        var native = await RecognizeNativeWithConfidenceAsync(image, cancellationToken);

        if (!ShouldTryLocalAi(native))
        {
            return native.Text;
        }

        var localAiText = await TryLocalAiAsync(image, cancellationToken);
        return string.IsNullOrWhiteSpace(localAiText)
            ? native.Text
            : localAiText;
    }

    private bool ShouldTryLocalAi(TextRecognitionResult nativeResult)
    {
        if (string.IsNullOrWhiteSpace(nativeResult.Text))
        {
            return true;
        }

        return nativeResult.Confidence < _nativeLowConfidenceThreshold;
    }

    private async Task<TextRecognitionResult> RecognizeNativeWithConfidenceAsync(
        CapturedScreenImage image,
        CancellationToken cancellationToken)
    {
        if (_nativeRecognizer is IConfidenceTextRecognizer confidenceTextRecognizer)
        {
            return await confidenceTextRecognizer.RecognizeWithConfidenceAsync(image, cancellationToken);
        }

        var nativeText = await _nativeRecognizer.RecognizeAsync(image, cancellationToken);
        var confidence = string.IsNullOrWhiteSpace(nativeText) ? 0d : 1d;
        return new TextRecognitionResult(nativeText, confidence);
    }

    private async Task<string> TryLocalAiThenNativeAsync(CapturedScreenImage image, CancellationToken cancellationToken)
    {
        var localAiText = await TryLocalAiAsync(image, cancellationToken);
        return string.IsNullOrWhiteSpace(localAiText)
            ? await _nativeRecognizer.RecognizeAsync(image, cancellationToken)
            : localAiText;
    }

    private async Task<string?> TryLocalAiAsync(CapturedScreenImage image, CancellationToken cancellationToken)
    {
        try
        {
            return await _localAiRecognizer.RecognizeAsync(image, cancellationToken);
        }
        catch (Exception ex) when (IsRecoverableLocalAiFailure(ex, cancellationToken))
        {
            return null;
        }
    }

    private static bool IsRecoverableLocalAiFailure(Exception ex, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return false;
        }

        return ex switch
        {
            LocalAiUnavailableException => true,
            HttpRequestException => true,
            TimeoutException => true,
            TaskCanceledException => true,
            _ => false,
        };
    }
}
