// Copyright (C) 2026 Florian Krämer
// SPDX-License-Identifier: GPL-3.0-or-later
// See LICENSE for details.

namespace YtDlpUi.Core.Services;

public static class YouTubeCookiesValidator
{
    private const int MaxInspectBytes = 32 * 1024;
    private const string NetscapeHeader = "# Netscape HTTP Cookie File";

    public static (bool LooksValid, string? Warning) Inspect(string filePath)
    {
        if (!File.Exists(filePath))
            return (false, "Cookies file was not found.");

        var bytes = ReadPrefix(filePath, MaxInspectBytes);
        if (bytes.Length == 0)
            return (false, "Cookies file is empty.");

        var text = DecodePrefix(bytes);
        if (text.Contains(NetscapeHeader, StringComparison.Ordinal))
            return (true, null);

        return (false, "File does not look like a Netscape cookies.txt export. yt-dlp may still accept it.");
    }

    private static byte[] ReadPrefix(string filePath, int maxBytes)
    {
        using var stream = File.OpenRead(filePath);
        var length = (int)Math.Min(maxBytes, stream.Length);
        var buffer = new byte[length];
        var read = stream.Read(buffer, 0, length);
        if (read < length)
            Array.Resize(ref buffer, read);

        return buffer;
    }

    private static string DecodePrefix(byte[] bytes)
    {
        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            return System.Text.Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);

        return System.Text.Encoding.UTF8.GetString(bytes);
    }
}
