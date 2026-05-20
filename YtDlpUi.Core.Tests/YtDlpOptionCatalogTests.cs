using YtDlpUi.Core.Services;

namespace YtDlpUi.Core.Tests;

public sealed class YtDlpOptionCatalogTests
{
    [Fact]
    public void LoadEmbedded_ContainsCommonFlags()
    {
        var catalog = new YtDlpOptionCatalog();
        Assert.NotNull(catalog.FindByFlag("-f"));
        Assert.NotEmpty(catalog.GetSections());
    }
}
