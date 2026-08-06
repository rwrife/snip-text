namespace SnipText.Core;

public sealed class LocalAiUnavailableException : Exception
{
    public LocalAiUnavailableException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
