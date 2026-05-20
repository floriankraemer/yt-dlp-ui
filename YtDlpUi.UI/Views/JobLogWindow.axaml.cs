using Avalonia.Controls;
using Avalonia.Interactivity;

namespace YtDlpUi.UI.Views;

public sealed partial class JobLogWindow : Window
{
    public JobLogWindow()
    {
        InitializeComponent();
    }

    public JobLogWindow(string title, string logText) : this()
    {
        Title = title;
        LogTextBox.Text = logText;
    }

    private void Close_Click(object? sender, RoutedEventArgs e) => Close();
}
