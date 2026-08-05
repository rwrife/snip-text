using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using SnipText.Core;
using Xunit;

namespace SnipText.Tests;

public sealed class LocalAiVisionRecognizerTests
{
    [Fact]
    public async Task RecognizeAsync_PostsBase64ImageToConfiguredEndpoint()
    {
        var handler = new CaptureHandler();
        using var httpClient = new HttpClient(handler);
        var recognizer = new LocalAiVisionRecognizer(
            httpClient,
            "http://127.0.0.1:11434/v1/chat/completions",
            "minicpm-v",
            TimeSpan.FromSeconds(2));

        var text = await recognizer.RecognizeAsync(CreateImage());

        Assert.Equal("recognized text", text);
        Assert.Equal("/v1/models", handler.RequestUris[0].AbsolutePath);
        Assert.Equal("/v1/chat/completions", handler.RequestUris[1].AbsolutePath);

        using var payload = JsonDocument.Parse(handler.PostedBody);
        Assert.Equal("minicpm-v", payload.RootElement.GetProperty("model").GetString());

        var content = payload
            .RootElement
            .GetProperty("messages")[0]
            .GetProperty("content");

        var imageUrl = content[1]
            .GetProperty("image_url")
            .GetProperty("url")
            .GetString();

        Assert.NotNull(imageUrl);
        Assert.StartsWith("data:image/bmp;base64,", imageUrl, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RecognizeAsync_ParsesSegmentedContentResponse()
    {
        var handler = new SegmentedContentHandler();
        using var httpClient = new HttpClient(handler);
        var recognizer = new LocalAiVisionRecognizer(
            httpClient,
            "http://127.0.0.1:11434/v1/chat/completions");

        var text = await recognizer.RecognizeAsync(CreateImage());

        Assert.Equal($"line one{Environment.NewLine}line two", text);
    }

    private static CapturedScreenImage CreateImage()
    {
        return new CapturedScreenImage(new ScreenSelectionBounds(0, 0, 1, 1),
            new byte[]
            {
                32, 64, 128, 255,
            });
    }

    private sealed class CaptureHandler : HttpMessageHandler
    {
        public List<Uri> RequestUris { get; } = new();

        public string PostedBody { get; private set; } = string.Empty;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestUris.Add(request.RequestUri!);

            if (request.Method == HttpMethod.Get)
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"data\":[]}", Encoding.UTF8, "application/json"),
                };
            }

            PostedBody = await request.Content!.ReadAsStringAsync(cancellationToken);
            const string response = "{" +
                "\"choices\":[{" +
                "\"message\":{\"content\":\"recognized text\"}" +
                "}]" +
                "}";

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(response, Encoding.UTF8, "application/json"),
            };
        }
    }

    private sealed class SegmentedContentHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Method == HttpMethod.Get)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"data\":[]}", Encoding.UTF8, "application/json"),
                });
            }

            const string response = "{" +
                "\"choices\":[{" +
                "\"message\":{\"content\":[{" +
                "\"type\":\"text\",\"text\":\"line one\"},{" +
                "\"type\":\"text\",\"text\":\"line two\"}]}}]}";

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(response, Encoding.UTF8, "application/json"),
            });
        }
    }
}
