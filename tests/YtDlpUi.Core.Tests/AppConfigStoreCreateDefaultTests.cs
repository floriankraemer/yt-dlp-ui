using YtDlpUi.Core.Models;
using YtDlpUi.Core.Services;

namespace YtDlpUi.Core.Tests;

public sealed class AppConfigStoreCreateDefaultTests
{
    [Fact]
    public void NewAppConfiguration_DefaultsThemePreferenceToSystem()
    {
        var config = new AppConfiguration();
        Assert.Equal(ThemePreference.System, config.ThemePreference);
    }

    [Fact]
    public void CreateDefault_UsesProfileStore()
    {
        var root = Path.Combine(Path.GetTempPath(), "yt-dlp-ui-tests", Guid.NewGuid().ToString("N"));
        var profiles = new ProfileStore(root);
        var store = AppConfigStore.CreateDefault(profiles);
        Assert.EndsWith(".yt-dlp-ui", store.ConfigRoot);
    }
}
