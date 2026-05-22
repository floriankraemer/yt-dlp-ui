// Copyright (C) 2026 Florian Krämer
// SPDX-License-Identifier: GPL-3.0-or-later
// See LICENSE for details.

using YtDlpUi.Core.Services;

namespace YtDlpUi.Core.Tests;

public sealed class YtDlpSearchResultParserTests
{
    private readonly YtDlpSearchResultParser _parser = new();

    [Fact]
    public void Parse_MultipleEntries_ReturnsAllResults()
    {
        const string json = """
            {
              "entries": [
                {
                  "id": "abc123",
                  "title": "First Video",
                  "channel": "Channel One",
                  "webpage_url": "https://www.youtube.com/watch?v=abc123",
                  "thumbnails": [
                    { "url": "https://i.ytimg.com/vi/abc123/hqdefault.jpg", "width": 480 }
                  ]
                },
                {
                  "id": "def456",
                  "title": "Second Video",
                  "uploader": "Uploader Two"
                }
              ]
            }
            """;

        var results = _parser.Parse(json);

        Assert.Equal(2, results.Count);
        Assert.Equal("abc123", results[0].VideoId);
        Assert.Equal("First Video", results[0].Title);
        Assert.Equal("Channel One", results[0].Channel);
        Assert.Equal("https://www.youtube.com/watch?v=abc123", results[0].WatchUrl);
        Assert.Equal("def456", results[1].VideoId);
        Assert.Equal("Uploader Two", results[1].Channel);
        Assert.Contains("def456", results[1].ThumbnailUrl ?? string.Empty);
    }

    [Fact]
    public void Parse_SingleVideoObject_ReturnsOneResult()
    {
        const string json = """
            {
              "id": "solo1",
              "title": "Solo",
              "uploader": "Solo Channel"
            }
            """;

        var results = _parser.Parse(json);

        Assert.Single(results);
        Assert.Equal("solo1", results[0].VideoId);
        Assert.Equal("https://www.youtube.com/watch?v=solo1", results[0].WatchUrl);
    }

    [Fact]
    public void Parse_InvalidJson_ReturnsEmpty()
    {
        Assert.Empty(_parser.Parse("not json"));
        Assert.Empty(_parser.Parse(string.Empty));
    }

    [Fact]
    public void Parse_EntryWithoutId_IsSkipped()
    {
        const string json = """{ "entries": [ { "title": "No id" } ] }""";

        Assert.Empty(_parser.Parse(json));
    }
}
