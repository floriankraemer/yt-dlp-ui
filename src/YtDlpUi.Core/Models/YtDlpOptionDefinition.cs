// Copyright (C) 2026 Florian Krämer
// SPDX-License-Identifier: GPL-3.0-or-later
// See LICENSE for details.

namespace YtDlpUi.Core.Models;

public sealed class YtDlpOptionDefinition
{
    public required string Flag { get; init; }
    public required string Section { get; init; }
    public required string ValueType { get; init; }
    public string? DefaultValue { get; init; }
    public required string Tooltip { get; init; }
    public IReadOnlyList<string> Choices { get; init; } = [];
}
