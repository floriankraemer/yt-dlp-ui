// Copyright (C) 2026 Florian Krämer
// SPDX-License-Identifier: GPL-3.0-or-later
// See LICENSE for details.

using YtDlpUi.Core.Models;
using YtDlpUi.Core.Services;

namespace YtDlpUi.Core.Tests;

public sealed class YtDlpProgressParserTests
{
    private readonly YtDlpProgressParser _parser = new();

    [Fact]
    public void TryParse_ParsesCustomTemplateLine()
    {
        const string line = "PROGRESS=42.5%|SPEED=1.2MiB/s|ETA=00:15";
        var ok = _parser.TryParse(line, out var percent, out var speed, out var eta);
        Assert.True(ok);
        Assert.Equal(42.5, percent, 1);
        Assert.Equal("1.2MiB/s", speed);
        Assert.Equal("00:15", eta);
    }

    [Fact]
    public void TryParse_ParsesLegacyDownloadPrefixOnCustomLine()
    {
        const string line = "download:PROGRESS=10%|SPEED=500KiB/s|ETA=01:00";
        var ok = _parser.TryParse(line, out var percent, out var speed, out var eta);
        Assert.True(ok);
        Assert.Equal(10, percent);
        Assert.Equal("500KiB/s", speed);
        Assert.Equal("01:00", eta);
    }

    [Fact]
    public void TryParse_ParsesDefaultDownloadLine()
    {
        const string line = "[download]  12.5% of   10.00MiB at  1.23MiB/s ETA 00:05";
        var ok = _parser.TryParse(line, out var percent, out var speed, out var eta);
        Assert.True(ok);
        Assert.Equal(12.5, percent, 1);
        Assert.Equal("1.23MiB/s", speed);
        Assert.Equal("00:05", eta);
    }

    [Fact]
    public void TryParse_StripsAnsiCodes()
    {
        const string line = "\x1b[0;94mPROGRESS=  3.2%\x1b[0m|SPEED=1.00MiB/s|ETA=00:10";
        var ok = _parser.TryParse(line, out var percent, out _, out _);
        Assert.True(ok);
        Assert.Equal(3.2, percent, 1);
    }

    [Fact]
    public void TryParse_IgnoresUnavailablePercent()
    {
        const string line = "PROGRESS=N/A|SPEED=1.2MiB/s|ETA=N/A";
        var ok = _parser.TryParse(line, out var percent, out var speed, out var eta);
        Assert.True(ok);
        Assert.Equal(0, percent);
        Assert.Equal("1.2MiB/s", speed);
        Assert.Null(eta);
    }

    [Fact]
    public void TryParse_IgnoresUnrelatedLine()
    {
        Assert.False(_parser.TryParse("[info] downloading", out _, out _, out _));
    }

    [Fact]
    public void TryParse_DetectsExtractAudioAsPostProcessing()
    {
        const string line = "[ExtractAudio] Destination: /tmp/song.mp3";
        var ok = _parser.TryParse(line, downloadComplete: false, out var result);
        Assert.True(ok);
        Assert.Equal(DownloadProgressPhase.PostProcessing, result.Phase);
        Assert.Equal("Converting audio", result.ActivityLabel);
        Assert.True(result.UseIndeterminateProgress);
    }

    [Fact]
    public void TryParse_AfterDownloadComplete_TreatsTemplateLineAsPostProcessing()
    {
        const string line = "PROGRESS= 50.0%|SPEED=2MiB/s|ETA=00:05";
        var ok = _parser.TryParse(line, downloadComplete: true, out var result);
        Assert.True(ok);
        Assert.Equal(DownloadProgressPhase.PostProcessing, result.Phase);
        Assert.Equal(50, result.ProgressPercent);
        Assert.False(result.UseIndeterminateProgress);
    }

    [Fact]
    public void TryParse_PostProcessTemplateWithUnavailablePercent_IsIndeterminate()
    {
        const string line = "PROGRESS=N/A|SPEED=N/A|ETA=N/A";
        var ok = _parser.TryParse(line, downloadComplete: true, out var result);
        Assert.True(ok);
        Assert.Equal(DownloadProgressPhase.PostProcessing, result.Phase);
        Assert.Null(result.ProgressPercent);
        Assert.True(result.UseIndeterminateProgress);
    }
}
