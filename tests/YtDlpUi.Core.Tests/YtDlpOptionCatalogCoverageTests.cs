using YtDlpUi.Core.Services;

namespace YtDlpUi.Core.Tests;

public sealed class YtDlpOptionCatalogCoverageTests
{
    [Fact]
    public void Catalog_ExposesAllDefinitionsInSections()
    {
        var catalog = new YtDlpOptionCatalog();
        var all = catalog.GetAll();
        Assert.True(all.Count > 30);

        foreach (var option in all)
        {
            Assert.NotNull(catalog.FindByFlag(option.Flag));
            Assert.Contains(option.Section, catalog.GetSections());
            Assert.NotEmpty(catalog.GetBySection(option.Section));
        }
    }
}
