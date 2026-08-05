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

        HotkeyTextBox.Text = currentSettings.Hotkey.DisplayText;
        OcrLanguageTagTextBox.Text = currentSettings.OcrLanguageTag ?? string.Empty;
        OutputModeComboBox.SelectedItem = currentSettings.OutputMode;
        EnableLocalAiCheckBox.IsChecked = currentSettings.EnableLocalAi;
    }

    public SnipTextSettings? SavedSettings { get; private set; }

    private void SaveButton_OnClick(object sender, RoutedEventArgs e)
    {
        var hotkeyText = HotkeyTextBox.Text;

        if (!GlobalHotkeyParser.TryParse(hotkeyText, out var hotkey, out var error))
        {
            MessageBox.Show(this, error ?? "Invalid hotkey.", "snip-text settings", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var outputMode = OutputModeComboBox.SelectedItem is SnipTextOutputMode selectedMode
            ? selectedMode
            : SnipTextOutputMode.AutoCopy;

        SavedSettings = SnipTextSettings.Normalize(new SnipTextSettings
        {
            Hotkey = hotkey,
            OcrLanguageTag = string.IsNullOrWhiteSpace(OcrLanguageTagTextBox.Text)
                ? null
                : OcrLanguageTagTextBox.Text.Trim(),
            OutputMode = outputMode,
            EnableLocalAi = EnableLocalAiCheckBox.IsChecked == true,
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
