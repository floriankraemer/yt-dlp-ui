// Copyright (C) 2026 Florian Krämer
// SPDX-License-Identifier: GPL-3.0-or-later
// See LICENSE for details.

using YtDlpUi.Core.Models;

namespace YtDlpUi.Core.Abstractions;

public interface IYouTubeAccountService
{
    string? ResolveCookiesPath();

    YouTubeAccountStatus GetStatus();

    Task<YouTubeImportResult> ImportAsync(string sourcePath, CancellationToken cancellationToken = default);

    Task SignOutAsync(CancellationToken cancellationToken = default);

    Task<string?> TestCookiesAsync(string ytDlpPath, CancellationToken cancellationToken = default);
}
