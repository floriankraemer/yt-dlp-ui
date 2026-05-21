using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using YtDlpUi.UI.ViewModels;

namespace YtDlpUi.UI.Views;

public sealed partial class SearchWindow : Window
{
    private readonly SearchViewModel _viewModel;

    public SearchWindow()
    {
        InitializeComponent();
        _viewModel = App.Services.CreateSearchViewModel();
        DataContext = _viewModel;
        Opened += async (_, _) => await _viewModel.InitializeAsync();
    }

    private async void Search_Click(object? sender, RoutedEventArgs e) =>
        await _viewModel.SearchAsync();

    private async void SearchQuery_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            await _viewModel.SearchAsync();
    }

    private async void AddToQueue_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { DataContext: SearchResultViewModel row })
            return;

        await row.AddToQueueAsync();
    }

    private void Close_Click(object? sender, RoutedEventArgs e) => Close();
}
