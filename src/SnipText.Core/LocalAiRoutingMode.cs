namespace SnipText.Core;

public enum LocalAiRoutingMode
{
    NativeOnly = 0,
    AiOnly = 1,
    AiFallbackWhenNativeConfidenceLow = 2,
}
