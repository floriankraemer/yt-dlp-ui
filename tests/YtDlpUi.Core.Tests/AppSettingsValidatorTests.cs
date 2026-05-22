using YtDlpUi.Core.Models;
using YtDlpUi.Core.Services;

namespace YtDlpUi.Core.Tests;

public sealed class AppSettingsValidatorTests
{
    private readonly AppSettingsValidator _validator = new(new ExtraArgsTokenizer(), new DownloadFolderService());

    [Fact]
    public void Validate_RejectsInvalidConcurrency()
    {
        var config = new AppConfiguration { MaxConcurrentDownloads = 99 };
        var profile = new DownloadProfile { Id = "p", Name = "P" };
        var errors = _validator.Validate(config, profile);
        Assert.Contains(errors, e => e.Contains("Max concurrent"));
    }

    [Fact]
    public void Validate_RejectsUnbalancedExtraArgs()
    {
        var config = new AppConfiguration();
        var profile = new DownloadProfile { Id = "p", Name = "P", ExtraArgs = "\"bad" };
        var errors = _validator.Validate(config, profile);
        Assert.NotEmpty(errors);
    }
}
