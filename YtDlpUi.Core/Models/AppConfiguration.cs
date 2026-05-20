namespace YtDlpUi.Core.Models;

public sealed class AppConfiguration
{
    public int SchemaVersion { get; set; } = Constants.SchemaVersions.AppConfig;
    public string? YtDlpPath { get; set; }
    public string? FfmpegPath { get; set; }
    public string? JsRuntimeEngine { get; set; }
    public string? JsRuntimePath { get; set; }
    public string? DownloadFolder { get; set; }
    public int MaxConcurrentDownloads { get; set; } = Constants.AppPaths.DefaultMaxConcurrentDownloads;
    public string ActiveProfileId { get; set; } = Constants.AppPaths.DefaultProfileId;
    public int CatalogVersion { get; set; } = Constants.SchemaVersions.OptionCatalog;
    public string YtDlpReleaseTag { get; set; } = Constants.BinaryReleaseManifest.YtDlpReleaseTag;
    public string FfmpegBuildId { get; set; } = Constants.BinaryReleaseManifest.FfmpegBuildId;
    public Dictionary<string, double> QueueColumnWidths { get; set; } = new(StringComparer.Ordinal);
}
