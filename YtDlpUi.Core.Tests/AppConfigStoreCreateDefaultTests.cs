using YtDlpUi.Core.Services;

namespace YtDlpUi.Core.Tests;

public sealed class AppConfigStoreCreateDefaultTests
{
    [Fact]
    public void CreateDefault_UsesProfileStore()
    {
        var root = Path.Combine(Path.GetTempPath(), "yt-dlp-ui-tests", Guid.NewGuid().ToString("N"));
        var profiles = new ProfileStore(root);
        var store = AppConfigStore.CreateDefault(profiles);
        Assert.EndsWith(".yt-dlp-ui", store.ConfigRoot);
    }
}
