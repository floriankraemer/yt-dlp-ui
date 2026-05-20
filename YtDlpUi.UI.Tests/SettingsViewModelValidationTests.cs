using YtDlpUi.Core.Services;
using YtDlpUi.UI.ViewModels;

namespace YtDlpUi.UI.Tests;

public sealed class SettingsViewModelValidationTests : IDisposable
{
    private readonly string _root;
    private readonly SettingsViewModel _viewModel;

    public SettingsViewModelValidationTests()
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
    public async Task SaveAsync_InvalidConcurrency_Fails()
    {
        await _viewModel.LoadAsync();
        _viewModel.MaxConcurrentDownloads = 99;
        var saved = await _viewModel.SaveAsync();
        Assert.False(saved);
        Assert.NotNull(_viewModel.ValidationError);
    }

    [Fact]
    public async Task SelectProfileAsync_UpdatesCliPreview()
    {
        await _viewModel.LoadAsync();
        var firstPreview = _viewModel.CliPreview;
        if (_viewModel.Profiles.Count > 1)
        {
            await _viewModel.SelectProfileAsync(_viewModel.Profiles[1]);
            Assert.NotEqual(firstPreview, _viewModel.CliPreview);
        }
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
            Directory.Delete(_root, recursive: true);
    }
}
