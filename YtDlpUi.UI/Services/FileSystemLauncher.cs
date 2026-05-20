using System.Diagnostics;

namespace YtDlpUi.UI.Services;

public static class FileSystemLauncher
{
    public static bool TryOpenFile(string path)
    {
        if (!File.Exists(path))
            return false;

        try
        {
            if (OperatingSystem.IsWindows())
            {
                Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
                return true;
            }

            if (OperatingSystem.IsMacOS())
                return StartProcess("open", path);

            return StartProcess("xdg-open", path);
        }
        catch
        {
            return false;
        }
    }

    public static bool TryOpenLocation(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                if (OperatingSystem.IsWindows())
                    return StartProcess("explorer.exe", $"/select,\"{path}\"");

                if (OperatingSystem.IsMacOS())
                    return StartProcess("open", $"-R \"{path}\"");

                var directory = Path.GetDirectoryName(path);
                return directory is not null && StartProcess("xdg-open", directory);
            }

            if (!Directory.Exists(path))
                return false;

            if (OperatingSystem.IsWindows())
                return StartProcess("explorer.exe", path);

            if (OperatingSystem.IsMacOS())
                return StartProcess("open", path);

            return StartProcess("xdg-open", path);
        }
        catch
        {
            return false;
        }
    }

    private static bool StartProcess(string fileName, string arguments)
    {
        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = true,
        });

        return process is not null;
    }
}
