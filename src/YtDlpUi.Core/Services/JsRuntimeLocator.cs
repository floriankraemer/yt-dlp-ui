using YtDlpUi.Core.Abstractions;
using YtDlpUi.Core.Constants;
using YtDlpUi.Core.Models;

namespace YtDlpUi.Core.Services;

public sealed class JsRuntimeLocator : IJsRuntimeLocator
{
    public string? ResolvePath(AppConfiguration config)
    {
        if (string.IsNullOrWhiteSpace(config.JsRuntimeEngine)
            || !JsRuntimeEngines.IsSupported(config.JsRuntimeEngine))
            return null;

        var configured = ExecutablePathResolver.NormalizeIfExists(config.JsRuntimePath);
        if (configured is not null)
            return configured;

        return ExecutablePathResolver.FindOnPath(JsRuntimeEngines.GetExecutableName(config.JsRuntimeEngine));
    }
}
