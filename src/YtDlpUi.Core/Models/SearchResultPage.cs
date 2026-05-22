// Copyright (C) 2026 Florian Krämer
// SPDX-License-Identifier: GPL-3.0-or-later
// See LICENSE for details.

namespace YtDlpUi.Core.Models;

public sealed class SearchResultPage
{
    public required string Query { get; init; }
    public required IReadOnlyList<YouTubeSearchResult> Results { get; init; }
    public int Skip { get; init; }
    public bool HasMoreResults { get; init; }
}
