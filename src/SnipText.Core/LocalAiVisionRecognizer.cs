using System.Buffers.Binary;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text.Json;

namespace SnipText.Core;

public sealed class LocalAiVisionRecognizer : ITextRecognizer
{
    private const string DefaultPrompt = "Extract all visible text from this image exactly as written. Preserve line breaks.";
    private const string DefaultModel = "minicpm-v";

    private readonly HttpClient _httpClient;
    private readonly Uri _chatCompletionsEndpoint;
    private readonly string _model;

    public LocalAiVisionRecognizer(
        HttpClient httpClient,
        string endpoint,
        string? model = null,
        TimeSpan? timeout = null)
    {
        ArgumentNullException.ThrowIfNull(httpClient);

        if (string.IsNullOrWhiteSpace(endpoint))
        {
            throw new ArgumentException("A local-AI endpoint is required.", nameof(endpoint));
        }

        if (!Uri.TryCreate(endpoint.Trim(), UriKind.Absolute, out var endpointUri))
        {
            throw new ArgumentException("The local-AI endpoint must be an absolute URL.", nameof(endpoint));
        }

        _httpClient = httpClient;
        _chatCompletionsEndpoint = endpointUri;
        _model = string.IsNullOrWhiteSpace(model) ? DefaultModel : model.Trim();

        RequestTimeout = timeout.GetValueOrDefault(TimeSpan.FromSeconds(8));
    }

    public TimeSpan RequestTimeout { get; }

    public async Task<string> RecognizeAsync(CapturedScreenImage image, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(image);

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(RequestTimeout);

        if (!await IsEndpointReachableAsync(timeoutCts.Token))
        {
            throw new LocalAiUnavailableException($"Local-AI endpoint is unreachable: {_chatCompletionsEndpoint}");
        }

        var responseText = await SendRecognitionRequestAsync(image, timeoutCts.Token);

        if (string.IsNullOrWhiteSpace(responseText))
        {
            throw new LocalAiUnavailableException("Local-AI endpoint returned an empty recognition response.");
        }

        return responseText;
    }

    private async Task<bool> IsEndpointReachableAsync(CancellationToken cancellationToken)
    {
        var probeUri = BuildProbeUri();

        try
        {
            using var probeRequest = new HttpRequestMessage(HttpMethod.Get, probeUri);
            using var probeResponse = await _httpClient.SendAsync(
                probeRequest,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            return true;
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return false;
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }

    private Uri BuildProbeUri()
    {
        var builder = new UriBuilder(_chatCompletionsEndpoint)
        {
            Path = "/v1/models",
            Query = string.Empty,
            Fragment = string.Empty,
        };

        return builder.Uri;
    }

    private async Task<string> SendRecognitionRequestAsync(CapturedScreenImage image, CancellationToken cancellationToken)
    {
        var imageData = Convert.ToBase64String(ToBmpBytes(image));
        var payload = new
        {
            model = _model,
            temperature = 0,
            messages = new object[]
            {
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new
                        {
                            type = "text",
                            text = DefaultPrompt,
                        },
                        new
                        {
                            type = "image_url",
                            image_url = new
                            {
                                url = $"data:image/bmp;base64,{imageData}",
                            },
                        },
                    },
                },
            },
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, _chatCompletionsEndpoint)
        {
            Content = JsonContent.Create(payload),
        };

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Local-AI request failed with {(int)response.StatusCode} {response.ReasonPhrase}: {body}");
        }

        return ParseResponseText(body);
    }

    private static string ParseResponseText(string body)
    {
        using var document = JsonDocument.Parse(body);

        if (!document.RootElement.TryGetProperty("choices", out var choices) ||
            choices.ValueKind != JsonValueKind.Array ||
            choices.GetArrayLength() == 0)
        {
            return string.Empty;
        }

        var message = choices[0].GetProperty("message");
        if (!message.TryGetProperty("content", out var content))
        {
            return string.Empty;
        }

        if (content.ValueKind == JsonValueKind.String)
        {
            return content.GetString()?.Trim() ?? string.Empty;
        }

        if (content.ValueKind != JsonValueKind.Array)
        {
            return string.Empty;
        }

        var textParts = new List<string>();

        foreach (var segment in content.EnumerateArray())
        {
            if (segment.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            if (!segment.TryGetProperty("text", out var textElement))
            {
                continue;
            }

            var text = textElement.GetString();
            if (!string.IsNullOrWhiteSpace(text))
            {
                textParts.Add(text.Trim());
            }
        }

        return string.Join(Environment.NewLine, textParts);
    }

    private static byte[] ToBmpBytes(CapturedScreenImage image)
    {
        var width = image.Width;
        var height = image.Height;
        var rowSize = checked(width * 4);
        var pixelDataSize = checked(rowSize * height);
        var fileSize = checked(54 + pixelDataSize);

        var output = new byte[fileSize];
        output[0] = (byte)'B';
        output[1] = (byte)'M';
        BinaryPrimitives.WriteInt32LittleEndian(output.AsSpan(2, 4), fileSize);
        BinaryPrimitives.WriteInt32LittleEndian(output.AsSpan(10, 4), 54);

        BinaryPrimitives.WriteInt32LittleEndian(output.AsSpan(14, 4), 40);
        BinaryPrimitives.WriteInt32LittleEndian(output.AsSpan(18, 4), width);
        BinaryPrimitives.WriteInt32LittleEndian(output.AsSpan(22, 4), height);
        BinaryPrimitives.WriteInt16LittleEndian(output.AsSpan(26, 2), 1);
        BinaryPrimitives.WriteInt16LittleEndian(output.AsSpan(28, 2), 32);
        BinaryPrimitives.WriteInt32LittleEndian(output.AsSpan(34, 4), pixelDataSize);
        BinaryPrimitives.WriteInt32LittleEndian(output.AsSpan(38, 4), 2835);
        BinaryPrimitives.WriteInt32LittleEndian(output.AsSpan(42, 4), 2835);

        var destinationOffset = 54;
        for (var y = height - 1; y >= 0; y--)
        {
            var sourceOffset = y * image.Stride;
            Buffer.BlockCopy(image.Bgra32Pixels, sourceOffset, output, destinationOffset, rowSize);
            destinationOffset += rowSize;
        }

        return output;
    }
}
