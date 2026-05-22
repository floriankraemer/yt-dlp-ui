using YtDlpUi.Core.Models;
using YtDlpUi.Core.Services;

namespace YtDlpUi.UI.ViewModels;

public sealed class DownloadJobViewModel : ViewModelBase
{
    private readonly DownloadJob _job;

    public DownloadJobViewModel(DownloadJob job) => _job = job;

    public Guid Id => _job.Id;
    public string Url => _job.Url;
    public string? Title => YtDlpMetadataParser.FormatQueueTitle(_job);
    public DownloadStatus Status => _job.Status;
    public string StatusText => Status switch
    {
        DownloadStatus.Running when _job.ProgressPhase == DownloadProgressPhase.PostProcessing => "Processing",
        _ => Status.ToString(),
    };
    public double Progress => _job.Progress;
    public bool IsProgressIndeterminate =>
        _job.Status == DownloadStatus.Running && _job.UseIndeterminateProgress;
    public string ProgressDisplayText => IsProgressIndeterminate
        ? _job.ProgressActivity ?? "Processing…"
        : $"{Progress:0}%";
    public string? Speed => _job.ProgressPhase == DownloadProgressPhase.PostProcessing
        ? _job.ProgressActivity ?? _job.Speed
        : _job.Speed;
    public string? Eta => _job.Eta;
    public string? Error => _job.Error;
    public string? LogOutput => _job.LogOutput;
    public bool CanViewLog => _job.Status == DownloadStatus.Failed;
    public bool CanStart => _job.Status is DownloadStatus.Queued or DownloadStatus.Failed;
    public bool CanCancel => _job.Status is DownloadStatus.Queued or DownloadStatus.Running;
    public bool CanRemove => _job.Status is DownloadStatus.Completed or DownloadStatus.Failed or DownloadStatus.Cancelled;
    public bool CanOpenOutput => DownloadOutputResolver.CanOpen(_job);
    public string OpenOutputMenuLabel => DownloadOutputResolver.IsSingleFile(_job) ? "Open file" : "Open Location";

    public void Refresh() => OnPropertyChanged(string.Empty);
}
