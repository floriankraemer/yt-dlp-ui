using YtDlpUi.Core.Constants;
using YtDlpUi.Core.Models;
using YtDlpUi.Core.Services;

namespace YtDlpUi.Core.Tests;

public sealed class SettingsMigrationTests
{
    [Fact]
    public void MigrateAppConfig_UpgradesSchemaVersion()
    {
        var config = new AppConfiguration { SchemaVersion = 0 };
        var migrated = SettingsMigration.MigrateAppConfig(config);
        Assert.Equal(SchemaVersions.AppConfig, migrated.SchemaVersion);
    }

    [Fact]
    public void MigrateProfile_UpgradesSchemaVersion()
    {
        var profile = new DownloadProfile { Id = "p", Name = "P", SchemaVersion = 0 };
        var migrated = SettingsMigration.MigrateProfile(profile);
        Assert.Equal(SchemaVersions.Profile, migrated.SchemaVersion);
    }
}
