// Copyright (C) 2026 Florian Krämer
// SPDX-License-Identifier: GPL-3.0-or-later
// See LICENSE for details.

using YtDlpUi.Core.Abstractions;
using YtDlpUi.Core.Constants;
using YtDlpUi.Core.Models;

namespace YtDlpUi.Core.Services;

public sealed class YouTubeAccountService : IYouTubeAccountService
{
    private const string CookiesTestUrl = "https://www.youtube.com/feed/subscriptions";

    private readonly string _cookiesFolder;
    private readonly string _cookiesFilePath;
    private readonly IYtDlpProcessRunner _processRunner;

    public YouTubeAccountService(string configRoot, IYtDlpProcessRunner processRunner)
    {
        _cookiesFolder = Path.Combine(configRoot, AppPaths.CookiesFolderName);
        _cookiesFilePath = Path.Combine(_cookiesFolder, AppPaths.YouTubeCookiesFileName);
        _processRunner = processRunner;
    }

    public string? ResolveCookiesPath() =>
        File.Exists(_cookiesFilePath) ? _cookiesFilePath : null;

    public YouTubeAccountStatus GetStatus() =>
        File.Exists(_cookiesFilePath) ? YouTubeAccountStatus.SignedIn : YouTubeAccountStatus.NotSignedIn;

    public async Task<YouTubeImportResult> ImportAsync(
        string sourcePath,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(sourcePath))
            return new YouTubeImportResult(false, "No cookies file was selected.", null);

        var fullSource = Path.GetFullPath(sourcePath.Trim());
        if (!File.Exists(fullSource))
            return new YouTubeImportResult(false, "The selected cookies file does not exist.", null);

        try
        {
            Directory.CreateDirectory(_cookiesFolder);
            ConfigFilePermissions.ApplyRestrictedPermissions(_cookiesFolder, isDirectory: true);
            await Task.Run(() => File.Copy(fullSource, _cookiesFilePath, overwrite: true), cancellationToken);
            ConfigFilePermissions.ApplyRestrictedPermissions(_cookiesFilePath, isDirectory: false);

            var (looksValid, warning) = YouTubeCookiesValidator.Inspect(_cookiesFilePath);
            return new YouTubeImportResult(true, null, looksValid ? null : warning);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return new YouTubeImportResult(false, ex.Message, null);
        }
    }

    public Task SignOutAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (File.Exists(_cookiesFilePath))
            File.Delete(_cookiesFilePath);

        return Task.CompletedTask;
    }

    public async Task<string?> TestCookiesAsync(string ytDlpPath, CancellationToken cancellationToken = default)
    {
        var cookiesPath = ResolveCookiesPath();
        if (cookiesPath is null)
            return "Not signed in. Import a cookies.txt file first.";

        if (string.IsNullOrWhiteSpace(ytDlpPath) || !File.Exists(ytDlpPath))
            return "yt-dlp executable not found. Set a path in Settings → Binaries.";

        var invocation = new YtDlpInvocation
        {
            ExecutablePath = ytDlpPath,
            Arguments =
            [
                "--cookies",
                cookiesPath,
                "--simulate",
                "--skip-download",
                "--ignore-config",
                CookiesTestUrl,
            ],
        };

        var result = await _processRunner.RunAsync(invocation, cancellationToken: cancellationToken);
        if (result.ExitCode == 0)
            return "Cookies are valid.";

        var message = YtDlpFailureMessageBuilder.Build(result);
        return string.IsNullOrWhiteSpace(message)
            ? "Cookies test failed. The file may be expired or invalid."
            : message;
    }
}
