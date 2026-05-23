// Copyright (C) 2026 Florian Krämer
// SPDX-License-Identifier: GPL-3.0-or-later
// See LICENSE for details.

using YtDlpUi.Core.Abstractions;
using YtDlpUi.Core.Models;

namespace YtDlpUi.Core.Services;

public sealed class YtDlpSearchService : IYtDlpSearchService
{
    public const int PageSize = 20;

    private readonly IYtDlpProcessRunner _processRunner;
    private readonly IAppConfigStore _appConfigStore;
    private readonly IBinaryLocator _binaryLocator;
    private readonly IJsRuntimeLocator _jsRuntimeLocator;
    private readonly IYouTubeAccountService _youTubeAccountService;
    private readonly YtDlpSearchResultParser _parser;

    public YtDlpSearchService(
        IYtDlpProcessRunner processRunner,
        IAppConfigStore appConfigStore,
        IBinaryLocator binaryLocator,
        IJsRuntimeLocator jsRuntimeLocator,
        IYouTubeAccountService youTubeAccountService,
        YtDlpSearchResultParser? parser = null)
    {
        _processRunner = processRunner;
        _appConfigStore = appConfigStore;
        _binaryLocator = binaryLocator;
        _jsRuntimeLocator = jsRuntimeLocator;
        _youTubeAccountService = youTubeAccountService;
        _parser = parser ?? new YtDlpSearchResultParser();
    }

    public Task<SearchResultPage> SearchAsync(string query, CancellationToken cancellationToken = default) =>
        SearchAsync(query, skip: 0, cancellationToken);

    public async Task<SearchResultPage> SearchAsync(
        string query,
        int skip,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new InvalidOperationException("Enter a search query.");

        if (skip < 0)
            throw new ArgumentOutOfRangeException(nameof(skip));

        var config = await _appConfigStore.LoadAsync(cancellationToken);
        var ytDlpPath = _binaryLocator.ResolveYtDlpPath(config)
            ?? throw new InvalidOperationException("yt-dlp was not found. Install it or set its path in Settings.");

        var trimmedQuery = query.Trim();
        var args = BuildSearchArguments(
            config,
            _jsRuntimeLocator,
            _youTubeAccountService.ResolveCookiesPath(),
            trimmedQuery,
            skip);
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
            Query = trimmedQuery,
            Results = results,
            Skip = skip,
            HasMoreResults = results.Count >= PageSize,
        };
    }

    public static IReadOnlyList<string> BuildSearchArguments(
        Models.AppConfiguration config,
        IJsRuntimeLocator jsRuntimeLocator,
        string query,
        int skip = 0) =>
        BuildSearchArguments(config, jsRuntimeLocator, cookiesPath: null, query, skip);

    public static IReadOnlyList<string> BuildSearchArguments(
        Models.AppConfiguration config,
        IJsRuntimeLocator jsRuntimeLocator,
        string? cookiesPath,
        string query,
        int skip = 0)
    {
        var end = skip + PageSize;
        var args = new List<string>
        {
            "--flat-playlist",
            "--dump-single-json",
            "--skip-download",
            "--ignore-config",
            "--playlist-start",
            (skip + 1).ToString(System.Globalization.CultureInfo.InvariantCulture),
            "--playlist-end",
            end.ToString(System.Globalization.CultureInfo.InvariantCulture),
        };

        if (!string.IsNullOrWhiteSpace(cookiesPath) && File.Exists(cookiesPath))
        {
            args.Add("--cookies");
            args.Add(cookiesPath);
        }

        var jsRuntimes = JsRuntimeArgumentBuilder.Build(config, jsRuntimeLocator);
        if (!string.IsNullOrWhiteSpace(jsRuntimes))
        {
            args.Add("--js-runtimes");
            args.Add(jsRuntimes);
        }

        args.Add($"ytsearch{end}:{query}");
        return args;
    }
}
