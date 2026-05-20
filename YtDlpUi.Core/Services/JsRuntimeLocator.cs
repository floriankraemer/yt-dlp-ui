using YtDlpUi.Core.Constants;
using YtDlpUi.Core.Models;

namespace YtDlpUi.Core.Services;

public sealed class JsRuntimeLocator
{
    public string? ResolvePath(AppConfiguration config)
    {
        if (string.IsNullOrWhiteSpace(config.JsRuntimeEngine)
            || !JsRuntimeEngines.IsSupported(config.JsRuntimeEngine))
            return null;

        var configured = NormalizeIfExists(config.JsRuntimePath);
        if (configured is not null)
            return configured;

        return FindOnPath(JsRuntimeEngines.GetExecutableName(config.JsRuntimeEngine));
    }

    private static string? NormalizeIfExists(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        var fullPath = Path.GetFullPath(path.Trim());
        return File.Exists(fullPath) ? fullPath : null;
    }

    private static string? FindOnPath(string executable)
    {
        var pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(pathEnv))
            return null;

        foreach (var folder in pathEnv.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            var candidate = Path.Combine(folder.Trim(), executable);
            if (File.Exists(candidate))
                return Path.GetFullPath(candidate);

            if (OperatingSystem.IsWindows())
            {
                var withExtension = candidate + ".exe";
                if (File.Exists(withExtension))
                    return Path.GetFullPath(withExtension);
            }
        }

        return null;
    }
}
