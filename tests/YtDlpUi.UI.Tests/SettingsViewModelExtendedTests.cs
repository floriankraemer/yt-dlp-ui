using YtDlpUi.Core.Services;
using YtDlpUi.UI.ViewModels;

namespace YtDlpUi.UI.Tests;

public sealed class SettingsViewModelExtendedTests : IDisposable
{
    private readonly string _root;
    private readonly SettingsViewModel _viewModel;
    private readonly ProfileStore _profiles;
    private readonly AppConfigStore _appConfig;

    public SettingsViewModelExtendedTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "yt-dlp-ui-tests", Guid.NewGuid().ToString("N"));
        _profiles = new ProfileStore(_root);
        _appConfig = new AppConfigStore(_root, _profiles);
        var catalog = new YtDlpOptionCatalog();
        var coordinator = new SettingsCoordinator(
            _appConfig,
            _profiles,
            new YtDlpCommandBuilder(catalog, new ExtraArgsTokenizer()),
            ViewModelTestHelpers.CreateValidator(),
            new BinaryLocator(_root),
            new JsRuntimeLocator());
        _viewModel = ViewModelTestHelpers.CreateSettingsViewModel(coordinator, _profiles, _root);
    }

    [Fact]
    public async Task CreateProfileAsync_AddsProfile()
    {
        await _viewModel.LoadAsync();
        _viewModel.NewProfileName = "Music";
        await _viewModel.CreateProfileAsync();
        Assert.Contains(_viewModel.Profiles, p => p.Name == "Music");
    }

    [Fact]
    public async Task DuplicateProfileAsync_CreatesCopy()
    {
        await _viewModel.LoadAsync();
        var before = _viewModel.Profiles.Count;
        await _viewModel.DuplicateProfileAsync();
        Assert.Equal(before + 1, _viewModel.Profiles.Count);
    }

    [Fact]
    public async Task TestYtDlpAsync_ReturnsErrorWhenMissing()
    {
        await _viewModel.LoadAsync();
        var version = await _viewModel.TestYtDlpAsync();
        Assert.Contains("not found", version ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task TestFfmpegAsync_ReturnsErrorWhenMissing()
    {
        await _viewModel.LoadAsync();
        var version = await _viewModel.TestFfmpegAsync();
        Assert.Contains("not found", version ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ApplyTestResult_SetsStatusMessageOnSuccess()
    {
        _viewModel.ApplyTestResult("yt-dlp", "2024.12.23");
        Assert.Contains("2024.12.23", _viewModel.StatusMessage ?? string.Empty);
        Assert.Null(_viewModel.ValidationError);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
            Directory.Delete(_root, recursive: true);
    }
}
