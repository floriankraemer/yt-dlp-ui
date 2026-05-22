// Copyright (C) 2026 Florian Krämer
// SPDX-License-Identifier: GPL-3.0-or-later
// See LICENSE for details.

using YtDlpUi.Core.Models;
using YtDlpUi.UI.ViewModels;

namespace YtDlpUi.UI.Tests;

public sealed class OptionSectionViewModelTests
{
    [Fact]
    public void Constructor_ExposesOptions()
    {
        var option = new OptionItemViewModel(
            new YtDlpOptionDefinition
            {
                Flag = "-f",
                Section = "Video Format",
                ValueType = "string",
                Tooltip = "Format",
            },
            "best");

        var section = new OptionSectionViewModel("Video Format", [option]);
        Assert.Equal("Video Format", section.Name);
        Assert.Single(section.Options);
    }
}
