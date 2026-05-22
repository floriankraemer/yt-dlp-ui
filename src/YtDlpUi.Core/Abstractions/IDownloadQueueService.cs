// Copyright (C) 2026 Florian Krämer
// SPDX-License-Identifier: GPL-3.0-or-later
// See LICENSE for details.

using YtDlpUi.Core.Models;

namespace YtDlpUi.Core.Abstractions;

public interface IDownloadQueueService
{
    IReadOnlyList<DownloadJob> Jobs { get; }
    event EventHandler? JobsChanged;
    Task<DownloadJob> EnqueueAsync(string url, string profileId, CancellationToken cancellationToken = default);
    Task StartJobAsync(Guid jobId, CancellationToken cancellationToken = default);
    Task CancelAsync(Guid jobId, CancellationToken cancellationToken = default);
    Task RemoveAsync(Guid jobId, CancellationToken cancellationToken = default);
}
