namespace SnipText.Core;

public interface ISnipTextSettingsStore
{
    Task<SnipTextSettings> LoadAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(SnipTextSettings settings, CancellationToken cancellationToken = default);
}
