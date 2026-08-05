using System.Text.Json;

namespace SnipText.Core;

public sealed class JsonSnipTextSettingsStore : ISnipTextSettingsStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

    private readonly string _settingsPath;

    public JsonSnipTextSettingsStore(string? settingsPath = null)
    {
        _settingsPath = settingsPath ??
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "snip-text",
                "settings.json");
    }

    public async Task<SnipTextSettings> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_settingsPath))
        {
            return SnipTextSettings.Default;
        }

        try
        {
            await using var stream = File.OpenRead(_settingsPath);
            var settings = await JsonSerializer.DeserializeAsync<SnipTextSettings>(
                stream,
                SerializerOptions,
                cancellationToken);

            return SnipTextSettings.Normalize(settings);
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
        {
            return SnipTextSettings.Default;
        }
    }

    public async Task SaveAsync(SnipTextSettings settings, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var normalized = SnipTextSettings.Normalize(settings);
        var directory = Path.GetDirectoryName(_settingsPath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = File.Create(_settingsPath);
        await JsonSerializer.SerializeAsync(stream, normalized, SerializerOptions, cancellationToken);
    }
}
