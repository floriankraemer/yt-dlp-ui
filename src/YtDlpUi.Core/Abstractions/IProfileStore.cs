// Copyright (C) 2026 Florian Krämer
// SPDX-License-Identifier: GPL-3.0-or-later
// See LICENSE for details.

using YtDlpUi.Core.Models;

namespace YtDlpUi.Core.Abstractions;

public interface IProfileStore
{
    Task<IReadOnlyList<DownloadProfile>> ListAsync(CancellationToken cancellationToken = default);
    Task<DownloadProfile?> GetAsync(string profileId, CancellationToken cancellationToken = default);
    Task SaveAsync(DownloadProfile profile, CancellationToken cancellationToken = default);
    Task DeleteAsync(string profileId, CancellationToken cancellationToken = default);
    Task<DownloadProfile> DuplicateAsync(string profileId, CancellationToken cancellationToken = default);
}
