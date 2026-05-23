// Copyright (C) 2026 Florian Krämer
// SPDX-License-Identifier: GPL-3.0-or-later
// See LICENSE for details.

using YtDlpUi.Core.Models;
using YtDlpUi.Core.Services;

namespace YtDlpUi.Core.Tests;

public sealed class YtDlpCommandBuilderCookiesTests : IDisposable
{
    private readonly string _root;
    private readonly YtDlpCommandBuilder _builder = new(new YtDlpOptionCatalog(), new ExtraArgsTokenizer());

    public YtDlpCommandBuilderCookiesTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "yt-dlp-ui-cookies-builder", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_root);
    }

    [Fact]
    public void Build_IncludesCookiesWhenFileExists()
    {
        var cookiesPath = Path.Combine(_root, "youtube.txt");
        File.WriteAllText(cookiesPath, "# Netscape HTTP Cookie File\n");
        var profile = new DownloadProfile { Id = "p", Name = "P" };

        var args = _builder.Build(profile, null, null, cookiesPath, "https://example.com/v");

        var cookiesIndex = args.ToList().IndexOf("--cookies");
        Assert.True(cookiesIndex >= 0);
        Assert.Equal(cookiesPath, args[cookiesIndex + 1]);
        Assert.True(args.ToList().IndexOf("--ignore-config") < cookiesIndex);
    }

    [Fact]
    public void Build_OmitsCookiesWhenPathNull()
    {
        var profile = new DownloadProfile { Id = "p", Name = "P" };
        var args = _builder.Build(profile, null, null, cookiesPath: null, "https://example.com/v");

        Assert.DoesNotContain("--cookies", args);
    }

    [Fact]
    public void Build_OmitsCookiesWhenFileMissing()
    {
        var profile = new DownloadProfile { Id = "p", Name = "P" };
        var args = _builder.Build(profile, null, null, Path.Combine(_root, "missing.txt"), "https://example.com/v");

        Assert.DoesNotContain("--cookies", args);
    }

    [Fact]
    public void Build_OmitsCookiesWhenPathWhitespace()
    {
        var profile = new DownloadProfile { Id = "p", Name = "P" };
        var args = _builder.Build(profile, null, null, "   ", "https://example.com/v");

        Assert.DoesNotContain("--cookies", args);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
            Directory.Delete(_root, recursive: true);
    }
}
