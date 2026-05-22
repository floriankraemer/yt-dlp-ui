// Copyright (C) 2026 Florian Krämer
// SPDX-License-Identifier: GPL-3.0-or-later
// See LICENSE for details.

using YtDlpUi.Core.Constants;

namespace YtDlpUi.Core.Tests;

public sealed class PlatformBinaryAssetsTests
{
    [Theory]
    [InlineData("win-x64", "yt-dlp.exe")]
    [InlineData("win-arm64", "yt-dlp_arm64.exe")]
    [InlineData("linux-x64", "yt-dlp_linux")]
    [InlineData("linux-arm64", "yt-dlp_linux")]
    [InlineData("osx-x64", "yt-dlp")]
    [InlineData("osx-arm64", "yt-dlp")]
    public void GetYtDlpAsset_UsesPlatformSpecificFileName(string rid, string expectedFileName)
    {
        var asset = PlatformBinaryAssets.GetYtDlpAsset(rid);

        Assert.Equal(expectedFileName, asset.FileName);
        Assert.Contains("/latest/", asset.DownloadUrl, StringComparison.Ordinal);
        Assert.Equal(BinaryReleaseManifest.YtDlpReleasePageUrl, asset.ReleasePageUrl);
    }

    [Theory]
    [InlineData("win-x64", "ffmpeg-master-latest-win64-gpl.zip")]
    [InlineData("linux-arm64", "ffmpeg-master-latest-linux64-gpl.tar.xz")]
    [InlineData("osx-arm64", "ffmpeg-master-latest-macos64-gpl.zip")]
    public void GetFfmpegAsset_UsesPlatformSpecificFileName(string rid, string expectedFileName)
    {
        var asset = PlatformBinaryAssets.GetFfmpegAsset(rid);

        Assert.Equal(expectedFileName, asset.FileName);
        Assert.Contains("/latest/", asset.DownloadUrl, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("win-x64", "deno-x86_64-pc-windows-msvc.zip")]
    [InlineData("win-arm64", "deno-aarch64-pc-windows-msvc.zip")]
    [InlineData("linux-x64", "deno-x86_64-unknown-linux-gnu.zip")]
    [InlineData("linux-arm64", "deno-aarch64-unknown-linux-gnu.zip")]
    [InlineData("osx-x64", "deno-x86_64-apple-darwin.zip")]
    [InlineData("osx-arm64", "deno-aarch64-apple-darwin.zip")]
    public void GetDenoAsset_UsesPlatformSpecificFileName(string rid, string expectedFileName)
    {
        var asset = PlatformBinaryAssets.GetDenoAsset(rid);

        Assert.Equal(expectedFileName, asset.FileName);
        Assert.Contains("/latest/", asset.DownloadUrl, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("linux-musl-x64")]
    [InlineData("freebsd-x64")]
    public void GetYtDlpAsset_UnsupportedPlatform_Throws(string rid)
    {
        var ex = Assert.Throws<UnsupportedPlatformException>(() => PlatformBinaryAssets.GetYtDlpAsset(rid));
        Assert.Equal(BinaryReleaseManifest.YtDlpReleasePageUrl, ex.ReleasePageUrl);
    }
}
