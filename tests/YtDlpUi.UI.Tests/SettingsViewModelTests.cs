using YtDlpUi.Core.Services;
using YtDlpUi.UI.ViewModels;

namespace YtDlpUi.UI.Tests;

public sealed class SettingsViewModelTests : IDisposable
{
    private readonly string _root;
    private readonly SettingsViewModel _viewModel;

    public SettingsViewModelTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "yt-dlp-ui-tests", Guid.NewGuid().ToString("N"));
        var profiles = new ProfileStore(_root);
        var appConfig = new AppConfigStore(_root, profiles);
        var catalog = new YtDlpOptionCatalog();
        var coordinator = new SettingsCoordinator(
            appConfig,
            profiles,
            new YtDlpCommandBuilder(catalog, new ExtraArgsTokenizer()),
            new AppSettingsValidator(new ExtraArgsTokenizer()),
            new BinaryLocator(_root),
            new JsRuntimeLocator());
        _viewModel = new SettingsViewModel(coordinator, catalog, profiles, new DownloadFolderService());
    }

    [Fact]
    public async Task LoadAsync_BuildsCliPreview()
    {
        await _viewModel.LoadAsync();
        Assert.False(string.IsNullOrWhiteSpace(_viewModel.CliPreview));
    }

    [Fact]
    public async Task SaveAsync_ValidProfile_Succeeds()
    {
        await _viewModel.LoadAsync();
        _viewModel.DownloadFolder = _root;
        var saved = await _viewModel.SaveAsync();
        Assert.True(saved);
        Assert.Null(_viewModel.ValidationError);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
            Directory.Delete(_root, recursive: true);
    }
}
