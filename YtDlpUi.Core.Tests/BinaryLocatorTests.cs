using YtDlpUi.Core.Models;
using YtDlpUi.Core.Services;

namespace YtDlpUi.Core.Tests;

public sealed class BinaryLocatorTests
{
    [Fact]
    public void ResolveYtDlpPath_PrefersConfiguredPath()
    {
        var root = Path.GetTempPath();
        var file = Path.GetTempFileName();
        try
        {
            File.WriteAllText(file, "x");
            var locator = new BinaryLocator(root);
            var config = new AppConfiguration { YtDlpPath = file };
            Assert.Equal(Path.GetFullPath(file), locator.ResolveYtDlpPath(config));
        }
        finally
        {
            File.Delete(file);
        }
    }
}
