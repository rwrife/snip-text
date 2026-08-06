using SnipText.Core;
using Xunit;

namespace SnipText.Tests;

public sealed class JsonSnipTextSettingsStoreTests
{
    [Fact]
    public async Task LoadAsync_ReturnsDefault_WhenFileMissing()
    {
        var settingsPath = Path.Combine(CreateTempDirectory(), "settings.json");
        var store = new JsonSnipTextSettingsStore(settingsPath);

        var settings = await store.LoadAsync();

        Assert.Equal(SnipTextSettings.Default, settings);
    }

    [Fact]
    public async Task SaveAndLoadAsync_RoundTripsSettings()
    {
        var settingsPath = Path.Combine(CreateTempDirectory(), "settings.json");
        var store = new JsonSnipTextSettingsStore(settingsPath);

        var expected = new SnipTextSettings
        {
            Hotkey = new GlobalHotkey(HotkeyModifiers.Control | HotkeyModifiers.Alt, 0x51),
            OcrLanguageTag = "en-US",
            OutputMode = SnipTextOutputMode.Preview,
            EnableLocalAi = true,
            LocalAiRoutingMode = LocalAiRoutingMode.AiOnly,
            LocalAiEndpoint = "http://127.0.0.1:8080/v1/chat/completions",
            LocalAiModel = "qwen2.5vl:7b",
            NativeLowConfidenceThreshold = 0.4,
        };

        await store.SaveAsync(expected);
        var loaded = await store.LoadAsync();

        Assert.Equal(expected, loaded);
    }

    [Fact]
    public async Task LoadAsync_ReturnsDefault_WhenJsonInvalid()
    {
        var settingsPath = Path.Combine(CreateTempDirectory(), "settings.json");
        await File.WriteAllTextAsync(settingsPath, "{not-valid-json");

        var store = new JsonSnipTextSettingsStore(settingsPath);

        var settings = await store.LoadAsync();

        Assert.Equal(SnipTextSettings.Default, settings);
    }

    [Fact]
    public async Task LoadAsync_NormalizesInvalidHotkeyToDefault()
    {
        var settingsPath = Path.Combine(CreateTempDirectory(), "settings.json");

        const string rawJson = """
        {
          "hotkey": {
            "modifiers": 0,
            "virtualKey": 79,
            "hasModifier": false,
            "displayText": "O"
          },
          "ocrLanguageTag": " en-US ",
          "outputMode": 1,
          "enableLocalAi": false,
          "localAiRoutingMode": 2,
          "localAiEndpoint": " http://127.0.0.1:11434/v1/chat/completions ",
          "localAiModel": " minicpm-v ",
          "nativeLowConfidenceThreshold": 1.5
        }
        """;

        await File.WriteAllTextAsync(settingsPath, rawJson);

        var store = new JsonSnipTextSettingsStore(settingsPath);
        var settings = await store.LoadAsync();

        Assert.Equal(SnipTextSettings.Default.Hotkey, settings.Hotkey);
        Assert.Equal("en-US", settings.OcrLanguageTag);
        Assert.Equal(SnipTextOutputMode.Preview, settings.OutputMode);
        Assert.Equal(LocalAiRoutingMode.AiFallbackWhenNativeConfidenceLow, settings.LocalAiRoutingMode);
        Assert.Equal("http://127.0.0.1:11434/v1/chat/completions", settings.LocalAiEndpoint);
        Assert.Equal("minicpm-v", settings.LocalAiModel);
        Assert.Equal(1d, settings.NativeLowConfidenceThreshold);
    }

    private static string CreateTempDirectory()
    {
        var directory = Path.Combine(Path.GetTempPath(), "snip-text-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        return directory;
    }
}
