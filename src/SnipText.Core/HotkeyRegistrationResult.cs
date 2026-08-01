namespace SnipText.Core;

public sealed record HotkeyRegistrationResult(bool Success, string? ErrorMessage = null)
{
    public static HotkeyRegistrationResult Ok() => new(true);

    public static HotkeyRegistrationResult Failed(string errorMessage) => new(false, errorMessage);
}
