using YtDlpUi.Core.Services;

namespace YtDlpUi.Core.Tests;

public sealed class ProfileStoreGetTests
{
    [Fact]
    public async Task GetAsync_WhenMissing_ReturnsNull()
    {
        var root = Path.Combine(Path.GetTempPath(), "yt-dlp-ui-tests", Guid.NewGuid().ToString("N"));
        var store = new ProfileStore(root);
        var profile = await store.GetAsync("missing");
        Assert.Null(profile);
    }
}
