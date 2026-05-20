using YtDlpUi.Core.Models;
using YtDlpUi.Core.Services;

namespace YtDlpUi.Core.Tests;

public sealed class AppSettingsValidatorPathTests
{
    [Fact]
    public void Validate_InvalidBinaryPaths_ReturnsErrors()
    {
        var validator = new AppSettingsValidator(new ExtraArgsTokenizer());
        var config = new AppConfiguration
        {
            YtDlpPath = "/does/not/exist/yt-dlp",
            FfmpegPath = "/does/not/exist/ffmpeg",
        };
        var profile = new DownloadProfile { Id = "p", Name = "Profile" };
        var errors = validator.Validate(config, profile);
        Assert.Equal(2, errors.Count);
    }
}
