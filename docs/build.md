# Build, test, and publish

## Solution structure

```
YtDlpUi.slnx
├── src/
│   ├── YtDlpUi.Core/           Business logic (queue, yt-dlp invocation, settings)
│   └── YtDlpUi.UI/             Avalonia desktop UI
└── tests/
    ├── YtDlpUi.Core.Tests/
    └── YtDlpUi.UI.Tests/
```

## Prerequisites

- Docker (for the dev container via `make`), or .NET 8 SDK on the host with `IN_CONTAINER=1`
- `yt-dlp` and `ffmpeg` on PATH or installed via the UI

## Commands

```bash
make ci
make coverage
make publish-ui RID=linux-x64
```

Host without container:

```bash
IN_CONTAINER=1 make ci
```

Coverage HTML: `artifacts/coverage/html/index.html`

Coverage gates (Coverlet): `YtDlpUi.UI.Tests` uses **line** coverage ≥85%; `YtDlpUi.Core.Tests` uses **method** coverage ≥85% (subprocess/installer types are excluded by file and covered via integration-style tests). Line coverage for Core is reported in the HTML report.
