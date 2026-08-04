using System.Windows;

namespace SnipText.Preview;

public partial class EditablePreviewWindow : Window
{
    public EditablePreviewWindow(string recognizedText)
    {
        ArgumentNullException.ThrowIfNull(recognizedText);

        InitializeComponent();
        RecognizedTextTextBox.Text = recognizedText;
        RecognizedTextTextBox.SelectAll();
        RecognizedTextTextBox.Focus();
    }

    private void CopyButton_OnClick(object sender, RoutedEventArgs e)
    {
        var text = RecognizedTextTextBox.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(text))
        {
            StatusTextBlock.Text = "Nothing to copy.";
            return;
        }

        Clipboard.SetText(text);
        StatusTextBlock.Text = $"Copied {text.Length} characters.";
    }

    private void CloseButton_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
