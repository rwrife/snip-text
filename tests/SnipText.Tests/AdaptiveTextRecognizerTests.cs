using SnipText.Core;
using Xunit;

namespace SnipText.Tests;

public sealed class AdaptiveTextRecognizerTests
{
    [Fact]
    public async Task NativeOnly_UsesNativeRecognizer()
    {
        var native = new StubConfidenceRecognizer("native", 0.2d);
        var localAi = new StubRecognizer(_ => "ai");
        var recognizer = new AdaptiveTextRecognizer(native, localAi, LocalAiRoutingMode.NativeOnly, 0.5d);

        var text = await recognizer.RecognizeAsync(CreateImage());

        Assert.Equal("native", text);
        Assert.Equal(1, native.Calls);
    }

    [Fact]
    public async Task AiOnly_UsesLocalAiWhenAvailable()
    {
        var native = new StubConfidenceRecognizer("native", 0.2d);
        var localAi = new StubRecognizer(_ => "ai");
        var recognizer = new AdaptiveTextRecognizer(native, localAi, LocalAiRoutingMode.AiOnly, 0.5d);

        var text = await recognizer.RecognizeAsync(CreateImage());

        Assert.Equal("ai", text);
        Assert.Equal(0, native.Calls);
    }

    [Fact]
    public async Task AiOnly_FallsBackToNative_WhenLocalAiUnavailable()
    {
        var native = new StubConfidenceRecognizer("native", 0.2d);
        var localAi = new StubRecognizer(_ => throw new HttpRequestException("offline"));
        var recognizer = new AdaptiveTextRecognizer(native, localAi, LocalAiRoutingMode.AiOnly, 0.5d);

        var text = await recognizer.RecognizeAsync(CreateImage());

        Assert.Equal("native", text);
        Assert.Equal(1, native.Calls);
    }

    [Fact]
    public async Task LowConfidenceMode_UsesNative_WhenConfidenceHigh()
    {
        var native = new StubConfidenceRecognizer("native", 0.9d);
        var localAi = new StubRecognizer(_ => "ai");
        var recognizer = new AdaptiveTextRecognizer(native, localAi, LocalAiRoutingMode.AiFallbackWhenNativeConfidenceLow, 0.5d);

        var text = await recognizer.RecognizeAsync(CreateImage());

        Assert.Equal("native", text);
        Assert.Equal(0, localAi.Calls);
    }

    [Fact]
    public async Task LowConfidenceMode_UsesLocalAi_WhenNativeConfidenceLow()
    {
        var native = new StubConfidenceRecognizer("native", 0.2d);
        var localAi = new StubRecognizer(_ => "ai");
        var recognizer = new AdaptiveTextRecognizer(native, localAi, LocalAiRoutingMode.AiFallbackWhenNativeConfidenceLow, 0.5d);

        var text = await recognizer.RecognizeAsync(CreateImage());

        Assert.Equal("ai", text);
        Assert.Equal(1, localAi.Calls);
    }

    [Fact]
    public async Task LowConfidenceMode_FallsBackToNative_WhenLocalAiFails()
    {
        var native = new StubConfidenceRecognizer("native", 0.2d);
        var localAi = new StubRecognizer(_ => throw new LocalAiUnavailableException("probe failed"));
        var recognizer = new AdaptiveTextRecognizer(native, localAi, LocalAiRoutingMode.AiFallbackWhenNativeConfidenceLow, 0.5d);

        var text = await recognizer.RecognizeAsync(CreateImage());

        Assert.Equal("native", text);
    }

    private static CapturedScreenImage CreateImage()
    {
        return new CapturedScreenImage(new ScreenSelectionBounds(0, 0, 2, 1),
            new byte[]
            {
                0, 0, 0, 255,
                255, 255, 255, 255,
            });
    }

    private sealed class StubRecognizer(Func<CapturedScreenImage, string> recognize) : ITextRecognizer
    {
        private readonly Func<CapturedScreenImage, string> _recognize = recognize;

        public int Calls { get; private set; }

        public Task<string> RecognizeAsync(CapturedScreenImage image, CancellationToken cancellationToken = default)
        {
            Calls++;
            return Task.FromResult(_recognize(image));
        }
    }

    private sealed class StubConfidenceRecognizer(string text, double confidence)
        : ITextRecognizer, IConfidenceTextRecognizer
    {
        private readonly string _text = text;
        private readonly double _confidence = confidence;

        public int Calls { get; private set; }

        public Task<string> RecognizeAsync(CapturedScreenImage image, CancellationToken cancellationToken = default)
        {
            Calls++;
            return Task.FromResult(_text);
        }

        public Task<TextRecognitionResult> RecognizeWithConfidenceAsync(
            CapturedScreenImage image,
            CancellationToken cancellationToken = default)
        {
            Calls++;
            return Task.FromResult(new TextRecognitionResult(_text, _confidence));
        }
    }
}
