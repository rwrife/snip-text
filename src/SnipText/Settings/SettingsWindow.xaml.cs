using System.Globalization;
using System.Windows;
using SnipText.Core;

namespace SnipText.Settings;

public partial class SettingsWindow : Window
{
    public SettingsWindow(SnipTextSettings currentSettings)
    {
        ArgumentNullException.ThrowIfNull(currentSettings);

        InitializeComponent();

        OutputModeComboBox.ItemsSource = Enum.GetValues<SnipTextOutputMode>();
        LocalAiRoutingModeComboBox.ItemsSource = Enum.GetValues<LocalAiRoutingMode>();

        HotkeyTextBox.Text = currentSettings.Hotkey.DisplayText;
        OcrLanguageTagTextBox.Text = currentSettings.OcrLanguageTag ?? string.Empty;
        OutputModeComboBox.SelectedItem = currentSettings.OutputMode;
        EnableLocalAiCheckBox.IsChecked = currentSettings.EnableLocalAi;

        LocalAiRoutingModeComboBox.SelectedItem = currentSettings.LocalAiRoutingMode;
        LocalAiEndpointTextBox.Text = currentSettings.LocalAiEndpoint;
        LocalAiModelTextBox.Text = currentSettings.LocalAiModel;
        NativeConfidenceThresholdTextBox.Text =
            currentSettings.NativeLowConfidenceThreshold.ToString("0.##", CultureInfo.InvariantCulture);
    }

    public SnipTextSettings? SavedSettings { get; private set; }

    private void SaveButton_OnClick(object sender, RoutedEventArgs e)
    {
        var hotkeyText = HotkeyTextBox.Text;

        if (!GlobalHotkeyParser.TryParse(hotkeyText, out var hotkey, out var error))
        {
            System.Windows.MessageBox.Show(this, error ?? "Invalid hotkey.", "snip-text settings", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        var outputMode = OutputModeComboBox.SelectedItem is SnipTextOutputMode selectedMode
            ? selectedMode
            : SnipTextOutputMode.AutoCopy;

        var routingMode = LocalAiRoutingModeComboBox.SelectedItem is LocalAiRoutingMode selectedRoutingMode
            ? selectedRoutingMode
            : LocalAiRoutingMode.AiFallbackWhenNativeConfidenceLow;

        if (!double.TryParse(
                NativeConfidenceThresholdTextBox.Text,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var threshold))
        {
            System.Windows.MessageBox.Show(
                this,
                "Native confidence threshold must be a number between 0 and 1.",
                "snip-text settings",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);
            return;
        }

        SavedSettings = SnipTextSettings.Normalize(new SnipTextSettings
        {
            Hotkey = hotkey,
            OcrLanguageTag = string.IsNullOrWhiteSpace(OcrLanguageTagTextBox.Text)
                ? null
                : OcrLanguageTagTextBox.Text.Trim(),
            OutputMode = outputMode,
            EnableLocalAi = EnableLocalAiCheckBox.IsChecked == true,
            LocalAiRoutingMode = routingMode,
            LocalAiEndpoint = LocalAiEndpointTextBox.Text,
            LocalAiModel = LocalAiModelTextBox.Text,
            NativeLowConfidenceThreshold = threshold,
        });

        DialogResult = true;
        Close();
    }

    private void CancelButton_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
