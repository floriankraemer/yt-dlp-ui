using YtDlpUi.Core.Constants;
using YtDlpUi.Core.Models;

namespace YtDlpUi.Core.Services;

public static class SettingsMigration
{
    public static AppConfiguration MigrateAppConfig(AppConfiguration config)
    {
        if (config.SchemaVersion >= SchemaVersions.AppConfig)
            return config;

        if (config.SchemaVersion < 3)
            config.ThemePreference = ThemePreference.System;

        config.SchemaVersion = SchemaVersions.AppConfig;
        return config;
    }

    public static DownloadProfile MigrateProfile(DownloadProfile profile)
    {
        if (profile.SchemaVersion >= SchemaVersions.Profile)
            return profile;

        profile.SchemaVersion = SchemaVersions.Profile;
        return profile;
    }
}
