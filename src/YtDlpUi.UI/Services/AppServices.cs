using YtDlpUi.Core.Abstractions;
using YtDlpUi.Core.Services;
using YtDlpUi.UI.ViewModels;

namespace YtDlpUi.UI.Services;

public sealed class AppServices
{
    public AppServices()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (string.IsNullOrWhiteSpace(home))
            home = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

        ConfigRoot = Path.Combine(home, Core.Constants.AppPaths.ConfigFolderName);
        ProfileStore = new ProfileStore(ConfigRoot);
        AppConfigStore = new AppConfigStore(ConfigRoot, ProfileStore);
        Catalog = new YtDlpOptionCatalog();
        ExtraArgsTokenizer = new ExtraArgsTokenizer();
        CommandBuilder = new YtDlpCommandBuilder(Catalog, ExtraArgsTokenizer);
        BinaryLocator = new BinaryLocator(ConfigRoot);
        JsRuntimeLocator = new JsRuntimeLocator();
        UrlNormalizer = new YouTubeUrlNormalizer();
        ProgressParser = new YtDlpProgressParser();
        OutputPathParser = new YtDlpOutputPathParser();
        MetadataParser = new YtDlpMetadataParser();
        Validator = new AppSettingsValidator(ExtraArgsTokenizer);
        ReleaseSource = new GitHubBinaryReleaseSource();
        DownloadHelper = new BinaryDownloadHelper();
        ProcessRunner = new YtDlpProcessRunner();
        DownloadFolderService = new DownloadFolderService();
        Queue = new DownloadQueueService(
            ProcessRunner,
            CommandBuilder,
            AppConfigStore,
            ProfileStore,
            BinaryLocator,
            UrlNormalizer,
            ProgressParser,
            OutputPathParser,
            MetadataParser,
            DownloadFolderService,
            JsRuntimeLocator);
        YtDlpInstaller = new YtDlpBinaryInstaller(ReleaseSource, BinaryLocator, DownloadHelper);
        FfmpegInstaller = new FfmpegBinaryInstaller(ReleaseSource, BinaryLocator, DownloadHelper);
    }

    public string ConfigRoot { get; }
    public IProfileStore ProfileStore { get; }
    public IAppConfigStore AppConfigStore { get; }
    public YtDlpOptionCatalog Catalog { get; }
    public ExtraArgsTokenizer ExtraArgsTokenizer { get; }
    public YtDlpCommandBuilder CommandBuilder { get; }
    public BinaryLocator BinaryLocator { get; }
    public JsRuntimeLocator JsRuntimeLocator { get; }
    public YouTubeUrlNormalizer UrlNormalizer { get; }
    public YtDlpProgressParser ProgressParser { get; }
    public YtDlpOutputPathParser OutputPathParser { get; }
    public YtDlpMetadataParser MetadataParser { get; }
    public AppSettingsValidator Validator { get; }
    public IBinaryReleaseSource ReleaseSource { get; }
    public BinaryDownloadHelper DownloadHelper { get; }
    public IYtDlpProcessRunner ProcessRunner { get; }
    public DownloadFolderService DownloadFolderService { get; }
    public IDownloadQueueService Queue { get; }
    public IBinaryInstaller YtDlpInstaller { get; }
    public IBinaryInstaller FfmpegInstaller { get; }

    public MainWindowViewModel CreateMainViewModel() => new(
        Queue,
        AppConfigStore,
        ProfileStore,
        DownloadFolderService,
        UrlNormalizer,
        YtDlpInstaller,
        FfmpegInstaller,
        BinaryLocator);

    public SettingsCoordinator CreateSettingsCoordinator() => new(
        AppConfigStore,
        ProfileStore,
        CommandBuilder,
        Validator,
        BinaryLocator,
        JsRuntimeLocator);

    public SettingsViewModel CreateSettingsViewModel() => new(
        CreateSettingsCoordinator(),
        Catalog,
        ProfileStore,
        DownloadFolderService);
}
