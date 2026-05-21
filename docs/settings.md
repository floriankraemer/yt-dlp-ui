# Settings and profiles

Configuration is stored under `~/.yt-dlp-ui/`:

- `app.json` — yt-dlp/ffmpeg paths, max concurrent downloads, theme, active profile
- `profiles/*.json` — named download profiles with common yt-dlp options

## Appearance

Open **Settings → Appearance** to choose **System**, **Light**, or **Dark**. **System** (default) follows your operating system theme on Windows, Linux, and macOS. **Light** and **Dark** override the OS setting. The choice is stored in `app.json` as `ThemePreference`.

## Binaries

Open **Settings → Binaries** in the sidebar to set paths to **yt-dlp**, **ffmpeg**, and a **JavaScript runtime** for YouTube, browse for executables, and run **Test** on each. Leave a path blank to use a bundled install (from the main window) or tools on your `PATH`. The resolved path shows what the app will actually run.

### JavaScript runtime (YouTube)

Since [yt-dlp 2025.11.12](https://github.com/yt-dlp/yt-dlp/issues/15012), a JS runtime (Deno recommended) is required for full YouTube support. Choose an engine in the dropdown and optionally set its executable path. The app passes `--js-runtimes ENGINE` or `--js-runtimes ENGINE:PATH` to yt-dlp. See the [EJS wiki](https://github.com/yt-dlp/yt-dlp/wiki/EJS) for install details.

| Engine | Notes |
|--------|--------|
| **Default** | Lets yt-dlp use its built-in lookup (Deno on PATH only) |
| **Deno** | Recommended |
| **Node** | Minimum v20 |
| **QuickJS / QuickJS-ng / Bun** | Optional alternatives |

## Download folder

On first launch you are asked to choose a download folder (default suggestion: `~/youtube-downloads`). Change it anytime under **Settings → Queue**. Profile option `-P` overrides this folder when set.

## Profiles

Built-in profiles (created on first run if missing):

| Profile | Purpose |
|---------|---------|
| **Default** | Single MP4 when available (`b[ext=mp4]/bv*+ba/b`), otherwise best merged video+audio |
| **Download Audio as mp3** | Like `downloadaudio2.ps1`: best audio → MP3, `%(channel)s/%(title)s`, SponsorBlock, `--force-overwrites` (avoids stale resume / HTTP 416) |
| **Download HQ Video** | Up to 1080p H.264/AAC, one `.mp4` file (progressive or merged) |

Each profile stores option values from the built-in catalog plus optional **Extra arguments** (quote-aware). Use Extra arguments for flags not exposed in the UI.

## Security

Profiles may contain cookies or passwords. Files are stored only on your machine. On Linux/macOS the app sets restrictive permissions where supported.

## Legal

You are responsible for complying with the terms of service of content sites and applicable laws. This app is a front-end for [yt-dlp](https://github.com/yt-dlp/yt-dlp).
