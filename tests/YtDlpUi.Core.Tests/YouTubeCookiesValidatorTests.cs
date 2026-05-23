// Copyright (C) 2026 Florian Krämer
// SPDX-License-Identifier: GPL-3.0-or-later
// See LICENSE for details.

using YtDlpUi.Core.Services;

namespace YtDlpUi.Core.Tests;

public sealed class YouTubeCookiesValidatorTests : IDisposable
{
    private readonly string _root;

    public YouTubeCookiesValidatorTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "yt-dlp-ui-cookie-validator", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_root);
    }

    [Fact]
    public void Inspect_AcceptsValidNetscapeHeader()
    {
        var path = Path.Combine(_root, "valid.txt");
        File.WriteAllText(path, "# Netscape HTTP Cookie File\n.youtube.com\tTRUE\t/\tFALSE\t0\tSID\tvalue\n");

        var (looksValid, warning) = YouTubeCookiesValidator.Inspect(path);

        Assert.True(looksValid);
        Assert.Null(warning);
    }

    [Fact]
    public void Inspect_WarnsWhenHeaderMissing()
    {
        var path = Path.Combine(_root, "invalid.txt");
        File.WriteAllText(path, ".youtube.com\tTRUE\t/\tFALSE\t0\tSID\tvalue\n");

        var (looksValid, warning) = YouTubeCookiesValidator.Inspect(path);

        Assert.False(looksValid);
        Assert.NotNull(warning);
    }

    [Fact]
    public void Inspect_IgnoresUtf8Bom()
    {
        var path = Path.Combine(_root, "bom.txt");
        var header = System.Text.Encoding.UTF8.GetBytes("# Netscape HTTP Cookie File\n");
        var bom = new byte[] { 0xEF, 0xBB, 0xBF };
        File.WriteAllBytes(path, bom.Concat(header).ToArray());

        var (looksValid, _) = YouTubeCookiesValidator.Inspect(path);

        Assert.True(looksValid);
    }

    [Fact]
    public void Inspect_EmptyFile_IsInvalid()
    {
        var path = Path.Combine(_root, "empty.txt");
        File.WriteAllText(path, string.Empty);

        var (looksValid, warning) = YouTubeCookiesValidator.Inspect(path);

        Assert.False(looksValid);
        Assert.NotNull(warning);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
            Directory.Delete(_root, recursive: true);
    }
}
