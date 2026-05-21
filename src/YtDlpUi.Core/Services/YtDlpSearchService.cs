using YtDlpUi.Core.Abstractions;
using YtDlpUi.Core.Models;

namespace YtDlpUi.Core.Services;

public sealed class YtDlpSearchService : IYtDlpSearchService
{
    public const int MaxResults = 20;

    private readonly IYtDlpProcessRunner _processRunner;
    private readonly IAppConfigStore _appConfigStore;
    private readonly BinaryLocator _binaryLocator;
    private readonly JsRuntimeLocator _jsRuntimeLocator;
    private readonly YtDlpSearchResultParser _parser;

    public YtDlpSearchService(
        IYtDlpProcessRunner processRunner,
        IAppConfigStore appConfigStore,
        BinaryLocator binaryLocator,
        JsRuntimeLocator jsRuntimeLocator,
        YtDlpSearchResultParser? parser = null)
    {
        _processRunner = processRunner;
        _appConfigStore = appConfigStore;
        _binaryLocator = binaryLocator;
        _jsRuntimeLocator = jsRuntimeLocator;
        _parser = parser ?? new YtDlpSearchResultParser();
    }

    public async Task<SearchResultPage> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new InvalidOperationException("Enter a search query.");

        var config = await _appConfigStore.LoadAsync(cancellationToken);
        var ytDlpPath = _binaryLocator.ResolveYtDlpPath(config)
            ?? throw new InvalidOperationException("yt-dlp was not found. Install it or set its path in Settings.");

        var args = BuildSearchArguments(config, _jsRuntimeLocator, query.Trim());
        var invocation = new YtDlpInvocation
        {
            ExecutablePath = ytDlpPath,
            Arguments = args,
        };

        var result = await _processRunner.RunAsync(invocation, cancellationToken: cancellationToken);
        if (result.ExitCode != 0)
        {
            var message = YtDlpFailureMessageBuilder.Build(result);
            throw new InvalidOperationException(
                string.IsNullOrWhiteSpace(message)
                    ? "YouTube search failed. Check that a JS runtime is configured in Settings."
                    : message);
        }

        var output = string.IsNullOrWhiteSpace(result.StandardOutput)
            ? result.StandardError
            : result.StandardOutput;

        var results = _parser.Parse(output);
        return new SearchResultPage
        {
            Query = query.Trim(),
            Results = results,
        };
    }

    public static IReadOnlyList<string> BuildSearchArguments(
        Models.AppConfiguration config,
        JsRuntimeLocator jsRuntimeLocator,
        string query)
    {
        var args = new List<string>
        {
            "--flat-playlist",
            "--dump-single-json",
            "--skip-download",
            "--ignore-config",
        };

        var jsRuntimes = JsRuntimeArgumentBuilder.Build(config, jsRuntimeLocator);
        if (!string.IsNullOrWhiteSpace(jsRuntimes))
        {
            args.Add("--js-runtimes");
            args.Add(jsRuntimes);
        }

        args.Add($"ytsearch{MaxResults}:{query}");
        return args;
    }
}
