using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

namespace YtDlpUi.UI.Views;

public sealed partial class DownloadFolderSetupWindow : Window
{
    public DownloadFolderSetupWindow()
    {
        InitializeComponent();
    }

    public DownloadFolderSetupWindow(string suggestedPath, string? errorMessage = null) : this()
    {
        FolderPathBox.Text = suggestedPath;
        if (!string.IsNullOrWhiteSpace(errorMessage))
            ErrorText.Text = errorMessage;
    }

    public string? SelectedPath { get; private set; }

    private async void Browse_Click(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel?.StorageProvider is not { } storageProvider)
            return;

        var folders = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select download folder",
            AllowMultiple = false,
        });

        if (folders.Count == 0)
            return;

        var path = folders[0].TryGetLocalPath();
        if (!string.IsNullOrWhiteSpace(path))
            FolderPathBox.Text = path;
    }

    private void Continue_Click(object? sender, RoutedEventArgs e)
    {
        ErrorText.Text = string.Empty;
        var path = FolderPathBox.Text?.Trim();
        if (string.IsNullOrWhiteSpace(path))
        {
            ErrorText.Text = "Please enter a download folder.";
            return;
        }

        SelectedPath = path;
        Close(true);
    }
}
