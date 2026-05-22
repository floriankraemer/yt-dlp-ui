// Copyright (C) 2026 Florian Krämer
// SPDX-License-Identifier: GPL-3.0-or-later
// See LICENSE for details.

using YtDlpUi.Core.Services;

namespace YtDlpUi.Core.Tests;

public sealed class ConfigFilePermissionsTests
{
    [Fact]
    public void ApplyRestrictedPermissions_DoesNotThrow()
    {
        var path = Path.Combine(Path.GetTempPath(), $"yt-dlp-ui-perm-{Guid.NewGuid():N}.json");
        File.WriteAllText(path, "{}");
        try
        {
            ConfigFilePermissions.ApplyRestrictedPermissions(path, isDirectory: false);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
