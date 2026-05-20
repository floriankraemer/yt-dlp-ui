using YtDlpUi.Core.Models;
using YtDlpUi.Core.Services;

namespace YtDlpUi.Core.Tests;

public sealed class AppSettingsValidatorSuccessTests
{
    [Fact]
    public void Validate_ValidConfiguration_ReturnsNoErrors()
    {
        var validator = new AppSettingsValidator(new ExtraArgsTokenizer());
        var config = new AppConfiguration { MaxConcurrentDownloads = 2 };
        var profile = new DownloadProfile { Id = "p", Name = "Profile" };
        Assert.Empty(validator.Validate(config, profile));
    }
}
