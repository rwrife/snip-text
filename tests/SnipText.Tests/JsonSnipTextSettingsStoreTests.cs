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
          "enableLocalAi": false
        }
        """;

        await File.WriteAllTextAsync(settingsPath, rawJson);

        var store = new JsonSnipTextSettingsStore(settingsPath);
        var settings = await store.LoadAsync();

        Assert.Equal(SnipTextSettings.Default.Hotkey, settings.Hotkey);
        Assert.Equal("en-US", settings.OcrLanguageTag);
        Assert.Equal(SnipTextOutputMode.Preview, settings.OutputMode);
    }

    private static string CreateTempDirectory()
    {
        var directory = Path.Combine(Path.GetTempPath(), "snip-text-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        return directory;
    }
}
